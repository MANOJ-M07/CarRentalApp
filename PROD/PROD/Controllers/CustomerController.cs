using DAL;
using PROD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using CostModel = PROD.Models.Cost;

namespace PROD.Controllers
{
    public class CustomerController : Controller
    {
        CarDAL cdal;
        CustomerDAL csdal;
        RentDAL rd;
        CarRentalEntities cd;

        // Default constructor 
        public CustomerController()
        {
            cdal = new CarDAL();
            cd = new CarRentalEntities();
            csdal = new CustomerDAL();
            rd = new RentDAL();          
        }
        // Generate a 5-digit random number 
        private string GenerateCaptcha()
        {
            return new Random().Next(10000, 99999).ToString();
        }
        // Login 
        public ActionResult Login()
        {
            Session["Captcha"] = GenerateCaptcha();
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection c)
        {
            // Retrieve login credentials from the form.
            var email = c["Email"];
            var password = c["Password"];
            var matchedCustomer = csdal.GetCustomers().FirstOrDefault(item => item.Email == email);
            if (matchedCustomer == null)
            {
                ViewBag.NotRegistered = true;
                return View();
            }
            else if (matchedCustomer.Password != password)
            {
                ViewBag.IncorrectPassword = true;
                return View();
            }
            if (Session["Captcha"] != null && Session["Captcha"].ToString() != c["CaptchaInput"])
            {
                ViewBag.CaptchaError = "CAPTCHA is incorrect.";
                return View();
            }
            TempData["User"] = matchedCustomer;
            Session["u1"] = matchedCustomer;
            return RedirectToAction("Search");
        }

        
        public ActionResult forgotpassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult forgotpassword(FormCollection c)
        {
            var email = c["Email"];
            var newPassword = c["Password"];
            //matching the email with the database 
            var customerToUpdate = csdal.GetCustomers().FirstOrDefault(item => item.Email == email);
            if (customerToUpdate == null)
            {
                ViewBag.ErrorMessage = "The provided email was not found. Please check the mail and try again.";
                return View();
            }
            customerToUpdate.Password = newPassword;
            if (csdal.UpdateCustomer(customerToUpdate.CustomerID, customerToUpdate))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // Registration
        public ActionResult Register()
        {
            // Create a new customer model instance
            var customer = new CustomerModel 
            {
                CustomerID = new Random().Next(1000, 40000),
                LoyaltyPoints = 0
            };
            Session["Captcha"] = GenerateCaptcha();
            return View(customer);
        }

        [HttpPost]
        public ActionResult Register(FormCollection c)
        {
            // Populate the Customer instance with the data from the form collection.
            var customer = new Customer
            {
                LoyaltyPoints = Convert.ToInt32(c["LoyaltyPoints"]),
                CustomerID = Convert.ToInt32(c["CustomerID"]),
                CustomerName = c["CustomerName"],
                Email = c["Email"],
                Password = c["Password"],
            };
            if (Session["Captcha"] != null && Session["Captcha"].ToString() != c["CaptchaInput"])
            {
                ViewBag.CaptchaError = "CAPTCHA is incorrect.";
                return View();
            }           
            try
            {
                if (csdal.AddCustomer(customer))
                {
                    return RedirectToAction("Login");
                }            
            }
            catch (ApplicationException ex)
            {
                if (ex.Message.Contains("A customer with this email address already exists"))
                {
                    ViewBag.Error = ex.Message;
                }
                else
                {
                    ViewBag.Error = "There was an issue registering the customer. Please try again.";
                }
                return View();
            }
            ViewBag.AddCustomerError = "There was an issue registering the customer. Please try again.";
            return View();
        }

        
        // Search functionality
        public ActionResult Search()
        {
            return View();
        }
        //search the date & time to rent a car
        [HttpPost]
        public ActionResult Search(SearchDates searchDates)
        {
            DateTime fullRentDate = searchDates.RentDate.Add(searchDates.RentTime);
            DateTime fullReturnDate = searchDates.ReturnDate.Add(searchDates.ReturnTime);

            if (!IsValidSearchDates(searchDates, out DateTime rentDate, out DateTime returnDate))
            {
                return View();
            }

            var customer = (Customer)TempData["user"];
            TempData["user"] = customer;  
            // Fetch all overlapping rentals for the given customer.
            var overlappingRentals = rd.GetAllRents().Where(x =>
            rentDate <= x.ReturnDate && returnDate >= x.RentOrderDate &&
            x.CustomerID == customer.CustomerID &&
            x.ReturnOdoReading is null).ToList();

            if (overlappingRentals.Any())
            {
                ViewBag.Message14 = "You have booked another car for the same exact time on that day";
                return View();
            }
            Session["RentDate"] = fullRentDate;
            Session["ReturnDate"] = fullReturnDate;
            return RedirectToAction("Index", new { rentDate = fullRentDate, returnDate = fullReturnDate });
        }

        // Utility methods
        private bool IsValidSearchDates(SearchDates searchDates, out DateTime rentDate, out DateTime returnDate)
        {
            rentDate = searchDates.RentDate.Add(searchDates.RentTime);
            returnDate = searchDates.ReturnDate.Add(searchDates.ReturnTime);

            DateTime currentDate = DateTime.Today;
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            if (searchDates.RentDate < currentDate)
            {
                ViewBag.Message13 = "Rent date cannot be in the past.";
                return false;
            }
            else if (searchDates.RentDate == currentDate && searchDates.RentTime < currentTime)
            {
                ViewBag.Message13 = "Rent time cannot be in the past.";
                return false;
            }
            if (returnDate < rentDate)
            {
                ViewBag.Message33 = "ReturnDate cannot be before the rent date and time.";
                return false;
            }
            return true;
        }
        

        // Get all current rentals.
        public List<int> Carlist()
        {
            DateTime k1 = Convert.ToDateTime(Session["RentDate"]);
            DateTime k2 = Convert.ToDateTime(Session["ReturnDate"]);
            SearchDates s = new SearchDates
            {
                RentDate = k1,
                ReturnDate = k2
            };
            List<Rental> m1 = rd.GetAllRents();
            // Filter rentals based on overlapping date range and if the car is not yet returned.
            m1 = m1.Where(x => (k1 <= x.ReturnDate && x.RentOrderDate <= k2) && x.ReturnOdoReading is null).ToList();
            List<int> m2 = m1.Select(item => Convert.ToInt32(item.CarID)).ToList();
            return m2;
        }

        public ActionResult Index()
        {
            try
            {
                List<Car> allCars = cdal.getcar() ?? new List<Car>();  
                if (allCars == null || !allCars.Any())
                {              
                    ViewBag.ErrorMessage = "Failed to retrieve the list of cars.";
                    return View(new List<CarModel>()); 
                }
                ViewBag.ImagePath = "~/images/";
                List<int> rentedCarIds = Carlist() ?? new List<int>();
                List<Car> availableCars = new List<Car>();
                foreach(var car in allCars)
                {
                    if(!rentedCarIds.Contains(car.CarID) && car.Available){
                        availableCars.Add(car);

                    }
                }
                List<CarModel> carsViewModel = availableCars.Select(car => new CarModel
                {
                    // Mapping properties from Car to CarViewModel
                    CarID = car.CarID,
                    CarName = car.CarName,
                    Available = car.Available,
                    PerDayCharge = car.PerDayCharge,
                    ChargePerKm = car.ChargePerKm,
                    CarType = car.CarType,
                    Photo = car.Photo
                }).ToList();
                return View(carsViewModel);
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View(new List<CarModel>());
            }
        }
        public ActionResult Details()
        {
            Customer g = TempData["User"] as Customer;
            if (g == null)
            {
                ViewBag.ErrorMessage = "Unable to retrieve user details. Please login again.";
                return View(new CustomerModel()); 
            }
            TempData["User"] = g; 
            int id = g.CustomerID;
            Customer k = csdal.GetCustomer(id);
            if (k == null)
            {
                ViewBag.ErrorMessage = "Unable to retrieve customer details.";
                return View(new CustomerModel()); 
            }
            // Convert the Customer entity to the CustomerViewModel 
            CustomerModel k1 = new CustomerModel
            {
                CustomerID = k.CustomerID,
                CustomerName = k.CustomerName,
                Password = k.Password,
                LoyaltyPoints = Convert.ToInt32(k.LoyaltyPoints),
                Email = k.Email
            };

            return View(k1);
        }
        public ActionResult Edit(int id)
        {
            //fetching customer details by using customer id
            Customer k = csdal.GetCustomer(id);
            if (k == null)
            {
                ViewBag.ErrorMessage = "Unable to find the customer for editing.";
                return View(new CustomerModel()); 
            }
            CustomerModel k1 = new CustomerModel
            {
                CustomerID = k.CustomerID,
                CustomerName = k.CustomerName,
                LoyaltyPoints = Convert.ToInt32(k.LoyaltyPoints),
                Email = k.Email
            };

            return View(k1);
        }

        [HttpPost]
        //updating the customer details by form data
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                if (!int.TryParse(collection["CustomerID"], out int customerId) ||
                    !int.TryParse(collection["LoyaltyPoints"], out int loyaltyPoints))
                {
                    ViewBag.ErrorMessage = "Invalid data provided.";
                    return View();
                }
                Customer k = new Customer
                {
                    CustomerID = customerId,
                    CustomerName = collection["CustomerName"],
                    Email = collection["Email"],
                    LoyaltyPoints = loyaltyPoints
                };
                bool k1 = csdal.UpdateCustomer(id, k);
                if (k1)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Unable to update customer details.";
                    return View();
                }
            }
            catch (Exception ex) 
            {               
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View();
            }
        }
        public ActionResult Rent(int id)
        {
            Car k2 = cdal.find(id);

            //calling the stored customer login details
            Customer k = Session["u1"] as Customer;
            if (k == null)
            {
                ViewBag.ErrorMessage = "Please login to rent a car.";
                return RedirectToAction("Login");
            }
            try
            {
                var customerInDb = csdal.GetCustomer(k.CustomerID);
                if (customerInDb == null)
                {
                    ViewBag.ErrorMessage = "Invalid customer details. Please login again.";
                    return RedirectToAction("Login");
                }
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return RedirectToAction("Rent");
            }
            DateTime? rentDateFromSession = Session["RentDate"] as DateTime?;
            DateTime? returnDateFromSession = Session["ReturnDate"] as DateTime?;
            if (!rentDateFromSession.HasValue || !returnDateFromSession.HasValue)
            {
                ViewBag.ErrorMessage = "Rent and return dates are not specified.";
                return RedirectToAction("Search");  
            }
            RentModel r = new RentModel();
            Random k1 = new Random();
            r.RentID = k1.Next(1000, 40000);
            r.CustomerID = k.CustomerID;
            r.CarID = id ;
            if (!k2.Available)
            {
                ViewBag.ErrorMessage = "The car you selected is not available for rent.";
                return RedirectToAction("Search"); 
            }
            ViewBag.image = "~/images/"+k2.Photo;
            r.RentOrderDate = Convert.ToDateTime(Session["RentDate"]);
            r.ReturnDate = Convert.ToDateTime(Session["ReturnDate"]);
            Session["RentDate"] = r.RentOrderDate;
            Session["ReturnDate"] = r.ReturnDate;
            return View(r);
        }
       
        [HttpPost]
        public ActionResult Rent(int id, RentModel model)
        {
            var r2 = model;
            if (r2 == null)
            {
                ViewBag.ErrorMessage = "Invalid rental information provided.";
                return View();
            }

            // Check if the car exists
            Car k2 = cdal.find(r2.CarID);
            if (k2 == null)
            {
                ViewBag.ErrorMessage = "Unable to find the specified car.";
                return View(r2);
            }
            if (!k2.Available)
            {
                ViewBag.ErrorMessage = "The car you selected is no longer available for rent.";
                return View(r2);
            }

            // Check if the customer exists
            var customerInDb = csdal.GetCustomer(r2.CustomerID);
            if (customerInDb == null)
            {
                ViewBag.ErrorMessage = "Invalid customer details.";
                return View(r2);
            }
            //convert RentModel to rent entity
            DAL.Rental r = MapToEntity(r2);
            bool result = false;
            try
            {
                result = rd.RentCar(r);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;  
            }
            if (result)
            {
                return RedirectToAction("PresentRentals", new {id = r2.RentID});
            }
            else
            {
                ViewBag.ErrorMessage = "Error while processing the rental.";
                return View(r2); 
            }
        }
        // transforms from the database format to the UI 
        private RentModel MapToViewModel(DAL.Rental entity)
        {
            if (entity == null) return null;

            return new RentModel
            {
                RentID = entity.RentID,
                CarID = entity.CarID,
                CustomerID = entity.CustomerID,
                RentOrderDate = entity.RentOrderDate, 
                ReturnDate = entity.ReturnDate, 
                OdoReading = entity.OdoReading,
                ReturnOdoReading = entity.ReturnOdoReading,
                LicenseNumber = entity.LicenseNumber
            };
        }
        // Helper method to map a RentalModel to a DAL Rental
        private DAL.Rental MapToEntity(RentModel viewModel)
        {
            if (viewModel == null) return null;

            return new DAL.Rental
            {
                RentID = viewModel.RentID,
                CarID = viewModel.CarID,
                CustomerID = viewModel.CustomerID,
                RentOrderDate = viewModel.RentOrderDate, 
                ReturnDate = viewModel.ReturnDate, 
                OdoReading = viewModel.OdoReading,
                ReturnOdoReading = viewModel.ReturnOdoReading,
                LicenseNumber = viewModel.LicenseNumber
            };
        }
        public ActionResult RentNow(int id)
        {
            // Fetch the rental record from the database using the provided ID.
            DAL.Rental rentEntity = rd.FindRent(id);
            if (rentEntity == null) return HttpNotFound();
            Car car = cdal.find(rentEntity.CarID);
            if (car == null) return HttpNotFound();

            ViewBag.image = "~/images/" + car.Photo;

            RentModel rentViewModel = MapToViewModel(rentEntity);
            return View(rentViewModel);
        }
        [HttpPost]
        public ActionResult RentNow(int id, RentModel rentViewModel)
        {            
            try
            {
                //fetch the records from DB
                DAL.Rental rentEntity = rd.FindRent(id);
                rentEntity.ReturnOdoReading = rentViewModel.ReturnOdoReading;
                rentEntity.OdoReading=rentViewModel.OdoReading;
                rd.ReturnCar(id, rentEntity); 
                return RedirectToAction("PresentRentals");
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View(rentViewModel);
            }
        }
        public ActionResult Pastrentals()
        {
            //fetching data from database
            List<Rental> ls = rd.GetAllRents();
            Customer k = TempData["user"] as Customer;

            if (k == null)
            {
                return RedirectToAction("Login");
            }
            int id = k.CustomerID;
            TempData["user"] = k;
            // Filter the rentals
            ls = ls.Where(x => (x.ReturnDate < DateTime.Today || x.ReturnOdoReading != null) && x.CustomerID == id).ToList();
            List<RentModel> list = new List<RentModel>();
            foreach (var rent in ls)
            {
                RentModel r = new RentModel
                {
                    RentID = rent.RentID,
                    CarID = rent.CarID,
                    CustomerID = rent.CustomerID,
                    RentOrderDate = rent.RentOrderDate,
                    ReturnDate = rent.ReturnDate,
                    OdoReading = rent.OdoReading,
                    ReturnOdoReading = rent.ReturnOdoReading,
                    LicenseNumber = rent.LicenseNumber
                };
                list.Add(r);
            }
            return View(list);
        }

        public ActionResult PresentRentals()
        {
            // Get the customer from session
            Customer k = Session["u1"] as Customer;

            if (k == null)
            {
                return RedirectToAction("Login");
            }

            int customerId = k.CustomerID;

            // Retrieve all renta record from DB
            List<DAL.Rental> ls = rd.GetAllRents();
            var count = ls.Count; 

            ls = ls.Where(x => x.ReturnDate >= DateTime.Today && x.CustomerID == customerId && x.ReturnOdoReading == null).ToList();
            var countAfterFilter = ls.Count;  
            List<RentModel> list = ls.Select(rent => new RentModel
            {
                RentID = rent.RentID,
                CarID = rent.CarID,
                CustomerID = rent.CustomerID,
                RentOrderDate = rent.RentOrderDate,
                ReturnDate = rent.ReturnDate,
                OdoReading = rent.OdoReading,
                ReturnOdoReading = rent.ReturnOdoReading,
                LicenseNumber = rent.LicenseNumber
            }).ToList();
            TempData["user"] = k;          
            return View(list); 
        }       
        public ActionResult CancelRent(int id)
        {
            try
            {
                Rental rent = rd.FindRent(id);
                if (rent == null)
                {                  
                    return RedirectToAction("Error", new { message = "Rental not found." });
                }
                //fetching the associated car for retrieved rental
                Car k2 = cdal.find(rent.CarID);
                if (k2 == null)
                {
                    return RedirectToAction("Error", new { message = "Car details not found for the rental." });
                }
                ViewBag.image = "~/images/" + k2.Photo;
                RentModel r = new RentModel
                {
                    RentID = rent.RentID,
                    CarID = rent.CarID,
                    CustomerID = rent.CustomerID,
                    RentOrderDate = rent.RentOrderDate,  
                    ReturnDate = rent.ReturnDate,        
                    OdoReading = rent.OdoReading,
                    ReturnOdoReading = rent.ReturnOdoReading,
                    LicenseNumber = rent.LicenseNumber
                };

                return View(r);
            }
            catch (Exception ex)
            {     
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CancelRent(int id,FormCollection collection)
        {
            try
            {
                // Check if the rental ID is valid
                if (id <= 0)
                {
                    return RedirectToAction("Error", new { message = "INvalid rental ID." });
                }
                rd.CancelRent(id);
                return RedirectToAction("PresentRentals");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        public ActionResult Return(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return RedirectToAction("Error", new { message = "Invalid ID" });
                }
                RentDAL rentDAL = new RentDAL();
                Rental rent = rentDAL.FindRent(id);
                if (rent == null)
                {
                    return RedirectToAction("Error", new { message = "Rental not found" });
                }
                // Create an instance of the CarDAL to fetch car details.
                CarDAL carDAL = new CarDAL();
                Car k2 = carDAL.find(rent.CarID);
                if (k2 == null)
                {
                    return RedirectToAction("Error", new { message = "Car not found" });
                }
                ViewBag.image = "~/images/"+ k2.Photo;
                RentModel rk = new RentModel()
                {
                    RentID=rent.RentID,
                    CarID=rent.CarID,
                    CustomerID=rent.CustomerID,
                    LicenseNumber=rent.LicenseNumber,
                    ReturnOdoReading=rent.ReturnOdoReading,
                    OdoReading=rent.OdoReading,
                    RentOrderDate=rent.RentOrderDate,
                    ReturnDate=rent.ReturnDate,
                };                
                return View(rk);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Return(int id, RentModel rentModel)
        {
            try
            {
                if (id <= 0)
                {
                    return RedirectToAction("Error", new { message = "Invalid ID" });
                }
                RentDAL rentDAL = new RentDAL();
                //using a mapping function to convert between models
                Rental rental = ConvertToRental(rentModel);
                rental.ReturnOdoReading = rentModel.ReturnOdoReading;
                rentDAL.ReturnCar(id, rental);
                return RedirectToAction("Payment", new {id=id});
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }
        private Rental ConvertToRental(RentModel rentModel)
        {
            return new Rental
            {
                RentID = rentModel.RentID,
                CarID = rentModel.CarID,
                CustomerID = rentModel.CustomerID,
                RentOrderDate = rentModel.RentOrderDate,
                ReturnDate = rentModel.ReturnDate,
                OdoReading = rentModel.OdoReading,
                ReturnOdoReading = rentModel.ReturnOdoReading,
                LicenseNumber = rentModel.LicenseNumber
            };
        }

        public ActionResult Payment(int id)
        {
            try
            {
                Rental rental = rd.FindRent(id);
                if (rental == null)
                {
                    return RedirectToAction("Error", new { message = "Rental not found." });
                }
                // Initialize a new CostModel to store the cost details
                CostModel costDetails = new CostModel
                {
                    RentID =rental.RentID,
                   
                };
                Tuple<int, double> charges = rd.CalculateCharges(rental);
                costDetails.KmsCovered = charges.Item1;
                int customerId = Convert.ToInt32(rental.CustomerID);
                csdal.AddLoyalty(costDetails.KmsCovered, customerId);
                decimal charge = Convert.ToDecimal(charges.Item2);
                decimal tax = 0M;
                if (charge < 1000M) tax = charge * 0.03M;
                else if (charge < 5000M) tax = charge * 0.05M;
                else tax = charge * 0.08M;
                // Assign values to the CostModel instance.
                costDetails.Price = charge;
                costDetails.Tax = tax;
                costDetails.TotalCost = charge + tax;
                TempData["Cos"] = costDetails;
                return View(costDetails);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }
        public ActionResult ApplyDiscount10(int id)
        {
            // retrieve the cost details from TempData
            CostModel costDetails = TempData["Cos"] as CostModel;
            if (costDetails == null)
            {
                return RedirectToAction("Error", new { message = "Cost details not found." });
            }
            Rental rental = rd.FindRent(id);
            if (rental == null)
            {
                return RedirectToAction("Error", new { message = "Rental not found." });
            }
            int customerId = Convert.ToInt32(rental.CustomerID);
            Customer customer = csdal.GetCustomer(customerId);
            if (customer.LoyaltyPoints >= 10)
            {
                const decimal DiscountRate = 0.1M; 
                costDetails.TotalCost = costDetails.TotalCost - (DiscountRate * costDetails.TotalCost);
                csdal.MinusLoyalty(customerId);
                ViewBag.Message = "Discount Applied.";
            }
            else
            {
                ViewBag.Message = "Your loyalty points have not reached the required level for a discount.";
            }

            return View(costDetails);
        }
        public ActionResult Successful()
        {
            return View();
        }

        public ActionResult preventBack()
        {
            if (TempData["User"] is Customer customer)
            {
                ViewData["status"] = customer.CustomerName;
            }
            else
            {
                ViewData["status"] = "Unknown";
            }
            Session["u1"] = null;
            return RedirectToAction("Login");
        }
    }
}