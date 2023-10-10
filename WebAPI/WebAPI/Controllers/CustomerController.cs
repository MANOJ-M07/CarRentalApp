
using DAL;
using PROD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    public class CustomerController : ApiController
    {
        CustomerDAL csdal = null;
        public CustomerController()
        {
            csdal = new CustomerDAL();
        }
        List<CustomerModel> c2 = new List<CustomerModel>();
        [Route("GetCustomer")]
        // GET: api/Customer
        public IEnumerable<CustomerModel> Get()
        {
            List<Customer> list = csdal.GetCustomers();
            foreach (var item in list)
            {
                CustomerModel c1 = new CustomerModel();
                c1.CustomerID = item.CustomerID;
                c1.CustomerName = item.CustomerName;
                c1.Email = item.Email;
                c1.Password = item.Password;
                c2.Add(c1);
            }
            return c2;
        }

        // GET: api/Customer/5
        [Route("FindCustomer/{id}")]
        public CustomerModel Get(int id)
        {
            var customer = csdal.GetCustomer(id);
            CustomerModel customerModel = new CustomerModel
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Password = customer.Password,

            };
            return customerModel;

        }

        [Route("AddCustomer")]
        public HttpResponseMessage Post([FromBody] CustomerModel value)
        {
            // Check if the input model is null
            if (value == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer data is required.");
            }

            // Check if the DAL object is initialized
            if (csdal == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Data access layer not initialized.");
            }

            try
            {
                Customer customer = new Customer
                {
                    CustomerID = value.CustomerID,
                    CustomerName = value.CustomerName,
                    Email = value.Email,
                    Password = value.Password
                    // ... (set any other properties here) ...
                };

                bool result = csdal.AddCustomer(customer);

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Failed to add customer.");
                }
            }
            catch (Exception ex)
            {
                // General error handling
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        // POST: api/Customer
        /* [Route("AddCustomer")]
         public HttpResponseMessage Post([FromBody]CustomerModel value)
         {
             Customer customer = new Customer();
             customer.CustomerID = value.CustomerID;
             customer.CustomerName = value.CustomerName;
             customer.Email = value.Email;
             customer.Password = value.Password;
             bool result = csdal.AddCustomer(customer);
             if (result == true)
             {
                 return Request.CreateResponse(HttpStatusCode.OK);
             }
             else
             {
                 return Request.CreateResponse(HttpStatusCode.NotAcceptable);
             }

         }*/

        // PUT: api/Customer/5
        [Route("UpdateCustomer/{id}")]
        public HttpResponseMessage Put(int id, [FromBody]CustomerModel value)
        {
            Customer customerToUpdate = new Customer();
            customerToUpdate.CustomerID = value.CustomerID;
            customerToUpdate.CustomerName = value.CustomerName;
            customerToUpdate.Email = value.Email;
            customerToUpdate.Password = value.Password;
            bool result = csdal.UpdateCustomer(id, customerToUpdate);
            if (result)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
        }

        // DELETE: api/Customer/5
        [Route("DeleteCustomer/{id}")]
        public HttpResponseMessage Delete(int id)
        {
            bool result = csdal.DeleteCustomer(id);
            if (result)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
        }
    }
}
