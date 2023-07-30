using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.BSE;
using Microsoft.Extensions.Logging;

namespace BusinessAccess.Services
{
    public class BSEIndicesService : IBSEIndicesService
    {
        private readonly ILogger<BSEIndicesService> _logger;
        private readonly IBSESensexDataAccess _bseSensexDataAccess;

        public BSEIndicesService(ILogger<BSEIndicesService> logger, IBSESensexDataAccess bseSensexDataAccess)
        {
            _logger = logger;
            _bseSensexDataAccess = bseSensexDataAccess;
        }

        public async Task<ServiceResponse<int>> DownloadIndicesInfo()
        {
            try
            {
                List<BSEIndex> indices = await GetIndices();
                return await _bseSensexDataAccess.SaveIndices(indices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DownloadIndicesInfo exception: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save BSE Indices Info", default);
        }

        private async Task<List<BSEIndex>> GetIndices()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.bseindia.com/BseIndiaAPI/api/FillddlIndex/w?fmdt=&todt=");

                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // execute the request
                Stream resStream = response.GetResponseStream(); // we will read data via the response stream

                var indices = await new StreamReader(resStream).ReadToEndAsync();

                return Newtonsoft.Json.JsonConvert.DeserializeObject<BSEIndices>(indices).Table;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetIndices exception: {ex.Message}");
            }
            return null;
        }

        public async Task<ServiceResponse<int>> DownloadIndicesData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<BSEIndex> indices = await GetIndices();
                await _bseSensexDataAccess.SaveIndices(indices);
                return await SaveIndicesData(indices, fromDate, toDate);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"DownloadIndicesData exception: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save BSE Indices Data", default);
        }

        private async Task<ServiceResponse<int>> SaveIndicesData(List<BSEIndex> indices, DateTime fromDate, DateTime toDate)
        {
            int count = 0, saved = 0;
            foreach (var index in indices)
            {
                try
                {
                    _logger.LogInformation($"Processing BSE Indices data for {index.Indx_cd} From Date: {fromDate.ToString("dd/MM/yyyy")}, To Date: {toDate.ToString("dd/MM/yyyy")}");
                    string url = $"https://api.bseindia.com/BseIndiaAPI/api/IndexArchDaily/w?period=D&index={ index.Indx_cd} &fmdt={fromDate.ToString("dd/MM/yyyy")}&todt={toDate.ToString("dd/MM/yyyy")}";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // execute the request
                    Stream resStream = response.GetResponseStream(); // we will read data via the response stream

                    var data = await new StreamReader(resStream).ReadToEndAsync();

                    var indicesData = Newtonsoft.Json.JsonConvert.DeserializeObject<BSEIndexData>(data).Table;

                    indicesData.ForEach(i =>
                        {
                            i.tdate = getDate(i);
                            i.I_name = i.I_name.Trim();
                        });

                    count += indicesData.Count;

                    var saveResponse = await _bseSensexDataAccess.SaveIndicesData(indicesData);
                    saved += saveResponse.ResponseObject;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to download data for {index.Indx_cd} From Date: {fromDate.ToString("dd/MM/yyyy")}, To Date: {toDate.ToString("dd/MM/yyyy")} exception: {ex.Message}");
                }

            }
            return new ServiceResponse<int>(count == saved, "BSE Indices Data Saved, Total: " + count.ToString() + " Saved: " + saved.ToString(), default);
        }

        private DateTime getDate(BSEIndexInfo i)
        {
            if (i.tdate == DateTime.MinValue)
            {
                int month = DateTime.Parse("1." + i.Month + " 2008").Month;
                return new DateTime(Convert.ToInt32(i.year), month, i.Day);
            }
            else
                return i.tdate;
        }

        public async Task<ServiceResponse<List<BSEIndexPerformance>>> GetPerformance()
        {
            List<int> periods = new List<int>() { 0, 1, 3, 6, 9, 12 };
            List<BSEIndexPerformance> response = new List<BSEIndexPerformance>();
            foreach (var period in periods)
            {
                ServiceResponse<DataTable> result = await _bseSensexDataAccess.GetPerformance(period);
                response = MapPerformance(result, response, period);
            }
            response.ForEach(r =>
            {
                r.MonthCloseGrowth = (r.CurrentClose - r.MonthClose) *100 / r.MonthClose;
                r.Month3CloseGrowth = (r.CurrentClose - r.Month3Close) * 100 / r.MonthClose;
                r.Month6CloseGrowth = (r.CurrentClose - r.Month6Close) * 100 / r.MonthClose;
                r.Month9CloseGrowth = (r.CurrentClose - r.Month9Close) * 100 / r.MonthClose;
                r.Month12CloseGrowth = (r.CurrentClose - r.Month12Close) * 100 / r.MonthClose;
            });
            return new ServiceResponse<List<BSEIndexPerformance>>(true, "Success", response);
        }

        private List<BSEIndexPerformance> MapPerformance(ServiceResponse<DataTable> result, List<BSEIndexPerformance> response, int period)
        {
            var r1 = (from r in result.ResponseObject.AsEnumerable()
                      select new BSEIndexPerformance()
                      {
                          Code = r["Code"].ToString(),
                          Alias = r["Alias"].ToString(),
                          CurrentClose = Convert.ToDecimal(r["Close"])
                      }).ToList();

            if (period == 0)
                response.AddRange(r1);
            else if (period == 1)
                response.ForEach(r => r.MonthClose = getValue(r.Code, r1));
            else if (period == 3)
                response.ForEach(r => r.Month3Close = getValue(r.Code, r1));
            else if (period == 6)
                response.ForEach(r => r.Month6Close = getValue(r.Code, r1));
            else if (period == 9)
                response.ForEach(r => r.Month9Close = getValue(r.Code, r1));
            else if (period == 12)
                response.ForEach(r => r.Month12Close = getValue(r.Code, r1));
            return response;
        }

        private decimal getValue(string code, List<BSEIndexPerformance> r1)
        {
            var filteredVal = r1.Where(r => r.Code == code).FirstOrDefault();
            return filteredVal == null ? 0 : filteredVal.CurrentClose;
        }
    }
}
