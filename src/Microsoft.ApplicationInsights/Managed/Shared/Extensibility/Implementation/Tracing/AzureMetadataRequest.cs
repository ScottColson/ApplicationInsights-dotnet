﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
#if NETSTANDARD1_3
    using System.Net.Http;
    using System.Runtime.InteropServices;
#endif
    using System.Threading.Tasks;

    internal class AzureMetadataRequest : IAzureMetadataRequestor, IHeartbeatDefaultPayloadProvider
    {
        // internal for testing
        internal readonly List<string> DefaultFields = new List<string>()
        {
            "osType",
            "location",
            "name",
            "offer",
            "platformFaultDomain",
            "platformUpdateDomain",
            "publisher",
            "sku",
            "version",
            "vmId",
            "vmSize"
        };

        /// <summary>
        /// Azure Instance Metadata Service exists on a single non-routable IP on machines configured
        /// by the Azure Resource Manager. See <a href="https://go.microsoft.com/fwlink/?linkid=864683">to learn more.</a>
        /// </summary>
        private static string baseImdsUrl = $"http://169.254.169.254/metadata/instance/compute";
        private static string imdsApiVersion = $"api-version=2017-04-02"; // this version has the format=text capability
        private static string imdsTextFormat = "format=text";

        private bool isAzureMetadataCheckCompleted = false;

        /// <summary>
        /// Gets all the available fields from the imds link asynchronously, and returns them as an IEnumerable.
        /// </summary>
        /// <returns>an array of field names available, or null</returns>
        public async Task<IEnumerable<string>> GetAzureInstanceMetadataComputeFields()
        {
            string allFieldsResponse = string.Empty;
            string metadataRequestUrl = $"{baseImdsUrl}?{imdsTextFormat}&{imdsApiVersion}";

            allFieldsResponse = await this.MakeAzureMetadataRequest(metadataRequestUrl);

            string[] fields = allFieldsResponse?.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length <= 0)
            {
                CoreEventSource.Log.CannotObtainAzureInstanceMetadata();
            }

            return fields;
        }

        /// <summary>
        /// Gets the value of a specific field from the imds link asynchronously, and returns it.
        /// </summary>
        /// <param name="fieldName">Specific imds field to retrieve a value for</param>
        /// <returns>an array of field names available, or null</returns>
        public async Task<string> GetAzureComputeMetadata(string fieldName)
        {
            string metadataRequestUrl = $"{baseImdsUrl}/{fieldName}?{imdsTextFormat}&{imdsApiVersion}";

            string requestResult = await this.MakeAzureMetadataRequest(metadataRequestUrl);

            return requestResult;
        }

        public bool IsKeyword(string keyword)
        {
            return this.DefaultFields.Contains(keyword, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> SetDefaultPayload(IEnumerable<string> disabledFields, IHeartbeatProvider provider)
        {
            bool hasSetFields = false;

            if (!this.isAzureMetadataCheckCompleted)
            {
                // only ever do this once when the SDK gets initialized
                this.isAzureMetadataCheckCompleted = true;

                var allFields = await this.GetAzureInstanceMetadataComputeFields()
                                .ConfigureAwait(false);

                var enabledImdsFields = this.DefaultFields.Except(disabledFields);
                foreach (string field in enabledImdsFields)
                {
                    provider.AddHeartbeatProperty(
                        propertyName: field,
                        overrideDefaultField: true,
                        propertyValue: await this.GetAzureComputeMetadata(field)
                            .ConfigureAwait(false),
                        isHealthy: true);
                    hasSetFields = true;
                }
            }

            return hasSetFields;
        }

        private async Task<string> MakeAzureMetadataRequest(string metadataRequestUrl)
        {
            string requestResult = string.Empty;

            SdkInternalOperationsMonitor.Enter();
            try
            {
#if NETSTANDARD1_3

                using (var getFieldValueClient = new HttpClient())
                {
                    getFieldValueClient.DefaultRequestHeaders.Add("Metadata", "True");
                    requestResult = await getFieldValueClient.GetStringAsync(metadataRequestUrl).ConfigureAwait(false);
                }

#elif NET45 || NET46

                WebRequest request = WebRequest.Create(metadataRequestUrl);
                request.Method = "GET";
                request.Headers.Add("Metadata", "true");
                using (WebResponse response = await request.GetResponseAsync().ConfigureAwait(false))
                {
                    if (response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader content = new StreamReader(httpResponse.GetResponseStream());
                        {
                            requestResult = content.ReadToEnd();
                        }
                    }
                }

#else
#error Unknown framework
#endif

            }
            catch (Exception ex)
            {
                CoreEventSource.Log.AzureInstanceMetadataRequestFailure(metadataRequestUrl, ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }
            finally
            {
                SdkInternalOperationsMonitor.Exit();
            }

            return requestResult;
        }
    }
}