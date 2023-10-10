using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DAL;
using PROD.Models;

namespace WebAPI.Controllers
{
    public class CarController : ApiController
    {
        // GET: api/Car
        CarDAL cdal = null;
        public CarController() {
           cdal = new CarDAL();
        }
        List<CarModel> c2 = new List<CarModel>();
        [Route("GetCar")]
        public  IEnumerable<CarModel> Get()
        {
            List<Car> list =cdal.getcar();
            foreach (Car item in list)
            {
                CarModel c1 = new CarModel();
                c1.CarID = item.CarID;  
                c1.CarName = item.CarName;
                c1.CarType = item.CarType;
                c1.ChargePerKm = item.ChargePerKm;
                c1.Available = item.Available;
                c1.PerDayCharge = item.PerDayCharge;
                c2.Add(c1);
            }
            return c2;
        }

        // GET: api/Car/5
       [Route("FindCar/{id}")]
        public CarModel Get(int id)
        {
            CarModel csdal = new CarModel();
            Car t = cdal.find(id);
            csdal.CarID = t.CarID;
            csdal.CarName = t.CarName;
            csdal.CarType = t.CarType;
            csdal.ChargePerKm= t.ChargePerKm;
            csdal.Available = t.Available;
            csdal.PerDayCharge = t.PerDayCharge;
            return csdal;

        }

        // POST: api/Car
        [Route("AddCar")]
        public HttpResponseMessage Post([FromBody]CarModel value)
        {
            Car c1 = new Car();
            /*c1.CarID = value.CarID;*/
            c1.CarName = value.CarName;
            c1.CarType = value.CarType;
            c1.ChargePerKm = value.ChargePerKm;
            c1.Available = value.Available;
            c1.PerDayCharge= value.PerDayCharge;
            bool k = cdal.addcar(c1);
            if (k==true)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
        }

        // PUT: api/Car/5
        [Route("UpdateCar/{id}")]
        public HttpResponseMessage Put(int id, [FromBody] CarModel value)
        {
            Car c1 = new Car();
            c1.CarID = value.CarID;
            c1.CarName = value.CarName;
            c1.CarType = value.CarType;
            c1.ChargePerKm = value.ChargePerKm;
            c1.Available = value.Available;
            c1.PerDayCharge = value.PerDayCharge;
           bool k= cdal.update(id, c1);
            if (k)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

        }


        // DELETE: api/Car/5
        [Route("delete/{id}")]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                bool k = cdal.delete(id);
                if (k)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Failed to delete car.");
                }
            }
            catch (ArgumentException ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (ApplicationException ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }



        /*// DELETE: api/Car/5
        [Route("delete/{id}")]
        public HttpResponseMessage Delete(int id)
        {
            bool k= cdal.delete(id);
            if(k)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
        }*/
    }
}
