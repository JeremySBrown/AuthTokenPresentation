using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApiFinal.Controllers
{
    [Authorize]
    public class SampleDataController : ApiController
    {
        public IHttpActionResult Get()
        {
            var result = GetSampleData();
            var user = User;
            var inRole = User.IsInRole("RoleB");
            return Ok(result);
        }

        public IHttpActionResult Get(int id)
        {
            var result = GetSampleData().FirstOrDefault(r => r.Id == id);
            if (result == null)
            {
                return BadRequest("No sample data found");
            }

            return Ok(result);
        }


        private IList<SampleData> GetSampleData()
        {
            var sample = new List<SampleData>();

            for (int i = 1; i <= 10; i++)
            {
                sample.Add(new SampleData
                {
                    Id = i,
                    Value = string.Format("Sample value for id of {0}", i)
                });
            }

            return sample;
        }
    }

    public class SampleData
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
