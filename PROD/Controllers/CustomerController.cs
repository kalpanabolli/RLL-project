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
    //Defining a controller name customer controller which in inherits from base class
    public class CustomerController : Controller
    {
        // instance of DAL and DB context
        CarDAL cdal;
        CarRentalEntities cd;
        CustomerDAL csdal;
        RentDAL rd;
       
        // Default constructor 
        public CustomerController()
        {
            //instantiate the DAL and DB context
            cdal = new CarDAL();
            cd = new CarRentalEntities();
            csdal = new CustomerDAL();
            rd = new RentDAL();
            
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
            // Check for a matching customer in the database.
            var matchedCustomer = csdal.GetCustomers().FirstOrDefault(item => item.Email == email);
            // If there's no matched customer
            if (matchedCustomer == null)
            {
                ViewBag.NotRegistered = true;
                return View();
            }
            // Case 2: Password does not match for the found email.
            else if (matchedCustomer.Password != password)
            {
                ViewBag.IncorrectPassword = true;
                return View();
            }

            // Validate the CAPTCHA
            if (Session["Captcha"] != null && Session["Captcha"].ToString() != c["CaptchaInput"])
            {
                ViewBag.CaptchaError = "CAPTCHA is incorrect.";
                return View();
            }
            //if login successful storing the date 
            TempData["User"] = matchedCustomer;
            Session["u1"] = matchedCustomer;

            return RedirectToAction("Search");
        }

        // Generate a 5-digit random number to act as a CAPTCHA.
        private string GenerateCaptcha()
        {
            return new Random().Next(10000, 99999).ToString();
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
                ViewBag.ErrorMessage = "The provided email was not found. Please check and try again.";
                return View();
            }
            //to update the password change in DB
            customerToUpdate.Password = newPassword;
            // Use the DAL to save the updated password.
            if (csdal.UpdateCustomer(customerToUpdate.CustomerID, customerToUpdate))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // Registration
        public ActionResult Register()
        {
            // Create a new customer model instance with a random customer ID and initial loyalty points set to 0.
            var customer = new CustomerModel 
            {
                CustomerID = new Random().Next(1000, 40000),
                LoyaltyPoints = 0
            };
            //storing the captcha for later validation
            Session["Captcha"] = GenerateCaptcha();
            return View(customer);
        }

        [HttpPost]
        public ActionResult Register(FormCollection c)
        {
            // Validate the CAPTCHA input from the user against the CAPTCHA stored in the session.
            if (Session["Captcha"] != null && Session["Captcha"].ToString() != c["CaptchaInput"])
            {
                ViewBag.CaptchaError = "CAPTCHA is incorrect.";
                return View();
            }
            // Populate the Customer instance with the data from the form collection.
            var customer = new Customer
            {
                LoyaltyPoints = Convert.ToInt32(c["LoyaltyPoints"]),
                CustomerID = Convert.ToInt32(c["CustomerID"]),
                CustomerName = c["CustomerName"],
                Email = c["Email"],
                Password = c["Password"],
            };
            try
            {
                // Use DAL to add the customer to the database.
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
        //The dates provided by the user for the car search
        //Redirects to the Index view if the search is successful, otherwise displays the Search view with relevant messages
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
            TempData["user"] = customer;  // Retaining the customer data in TempData for subsequent requests.

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

            // Storing the dates in Session
            Session["RentDate"] = fullRentDate;
            Session["ReturnDate"] = fullReturnDate;

            // Redirect to index to see the available cars
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
                // Rent date cannot be in the past.
                ViewBag.Message13 = "Rent date cannot be in the past.";
                return false;
            }
            else if (searchDates.RentDate == currentDate && searchDates.RentTime < currentTime)
            {
                // Rent time cannot be in the past for today.
                ViewBag.Message13 = "Rent time cannot be in the past.";
                return false;
            }

            if (returnDate < rentDate)
            {
                // Return date and time must be after the rent date and time.
                ViewBag.Message33 = "ReturnDate cannot be before the rent date and time.";
                return false;
            }

            return true;
        }
        

        // Get all current rentals.
        public List<int> Carlist()
        {
            // Retrieve RentDate and ReturnDate from Session and convert to DateTime
            DateTime k1 = Convert.ToDateTime(Session["RentDate"]);
            DateTime k2 = Convert.ToDateTime(Session["ReturnDate"]);

            // Create an instance of SearchDates using the fetched dates.
            SearchDates s = new SearchDates
            {
                RentDate = k1,
                ReturnDate = k2
            };
            //fetch all rental records
            List<Rental> m1 = rd.GetAllRents();

            // Filter rentals based on overlapping date range and if the car is not yet returned.
            m1 = m1.Where(x => (k1 <= x.ReturnDate && x.RentOrderDate <= k2) && x.ReturnOdoReading is null).ToList();

            // Extract the car IDs from the filtered rental records.
            List<int> m2 = m1.Select(item => Convert.ToInt32(item.CarID)).ToList();
            return m2;
        }

        public ActionResult Index()
        {
            try
            {
                // Get a list of all cars from DB using DAL
                List<Car> allCars = cdal.getcar() ?? new List<Car>();  // Default to an empty list if null is returned

                // Ensure the database call was successful
                if (allCars == null || !allCars.Any())
                {              
                    ViewBag.ErrorMessage = "Failed to retrieve the list of cars.";
                    return View(new List<CarModel>()); // Return an empty list to the view
                }

                ViewBag.ImagePath = "~/images/";
                // Fetch the list of cars that are currently rented.
                List<int> rentedCarIds = Carlist() ?? new List<int>();

                // Filter out the rented cars from all cars
                List<Car> availableCars = new List<Car>();
                foreach(var car in allCars)
                {
                    if(!rentedCarIds.Contains(car.CarID) && car.Available){
                        availableCars.Add(car);

                    }
                }

                // Convert the list of Car to CarViewModel
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

                // Unlock all available cars for rent
               
                //Send the list of available cars
                return View(carsViewModel);
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                //returning an empty list provides a fail-safe mechanism
                return View(new List<CarModel>());
            }
        }
        // Fetches the customer details and sends them to the details view.
        public ActionResult Details()
        {
            //fetching the customer details which we stored in the TempData
            Customer g = TempData["User"] as Customer;
            if (g == null)
            {
                // Handle error - No user data found in TempData
                ViewBag.ErrorMessage = "Unable to retrieve user details. Please login again.";
                return View(new CustomerModel()); // Empty view model
            }

            TempData["User"] = g; // Storing the user back in TempData
            int id = g.CustomerID;
            //fetching customer details by using customer id
            Customer k = csdal.GetCustomer(id);
            if (k == null)
            {
                ViewBag.ErrorMessage = "Unable to retrieve customer details.";
                return View(new CustomerModel()); // Empty view model
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
        //A view with the details of the customer to be edited.
        public ActionResult Edit(int id)
        {
            //fetching customer details by using customer id
            Customer k = csdal.GetCustomer(id);
            if (k == null)
            {
                ViewBag.ErrorMessage = "Unable to find the customer for editing.";
                return View(new CustomerModel()); // Empty view model
            }
            // Convert the Customer entity to the CustomerViewModel 
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
                 //passing data from the form data
                if (!int.TryParse(collection["CustomerID"], out int customerId) ||
                    !int.TryParse(collection["LoyaltyPoints"], out int loyaltyPoints))
                {
                    // Handle conversion errors
                    ViewBag.ErrorMessage = "Invalid data provided.";
                    return View();
                }
                //creating obj from the form data to update
                Customer k = new Customer
                {
                    CustomerID = customerId,
                    CustomerName = collection["CustomerName"],
                    Email = collection["Email"],
                    LoyaltyPoints = loyaltyPoints
                };
                //updating the customer details in DB
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
                // User not found in session, redirect to login
                ViewBag.ErrorMessage = "Please login to rent a car.";
                return RedirectToAction("Login");
            }
                    
            // Check if CustomerID exists in the Customers table
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

            // Retrieve the stored dates and times
            DateTime? rentDateFromSession = Session["RentDate"] as DateTime?;
            DateTime? returnDateFromSession = Session["ReturnDate"] as DateTime?;
            //checking the dats is correct 
            if (!rentDateFromSession.HasValue || !returnDateFromSession.HasValue)
            {
                ViewBag.ErrorMessage = "Rent and return dates are not specified.";
                return RedirectToAction("Search");  // Redirect them to provide dates again
            }
            //preparing the rent model for view
            RentModel r = new RentModel();
            Random k1 = new Random();
            r.RentID = k1.Next(1000, 40000);
            r.CustomerID = k.CustomerID;
            r.CarID = id ;
            //check if the car is available for rent
           
            
            // Add this check
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
        // This action processes the rental request.
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
            //if success redirect the user to presentRentals
            if (result)
            {
                return RedirectToAction("PresentRentals", new {id = r2.RentID});
            }
            else
            {
                ViewBag.ErrorMessage = "Error while processing the rental.";
                return View(r2);  // Return the model to preserve user's input
            }
        }

        // Helper method to map a DAL Rental to a RentModel
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
        // transforms from the UI format to the database 
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

            // Fetch the associated car details from the database.
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
                //update the readings in the view model
                rentEntity.ReturnOdoReading = rentViewModel.ReturnOdoReading;
                rentEntity.OdoReading=rentViewModel.OdoReading;
                //this update that reading to the DB
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
            //stores the user data back to temp data
            TempData["user"] = k;

            // Filter the rentals
            ls = ls.Where(x => (x.ReturnDate < DateTime.Today || x.ReturnOdoReading != null) && x.CustomerID == id).ToList();
            List<RentModel> list = new List<RentModel>();
            //maping the filtered rental to the view model
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
            var count = ls.Count;  // Count before filtering

            ls = ls.Where(x => x.ReturnDate >= DateTime.Today && x.CustomerID == customerId && x.ReturnOdoReading == null).ToList();
            var countAfterFilter = ls.Count;  // Count after filtering
                     
            // Map data to the view model
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

            // Store the logged-in customer details
            TempData["user"] = k;          
            return View(list); 
        }       
        public ActionResult CancelRent(int id)
        {
            try
            {
                // Retrieve the rental details from the database
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
                // Map the retrieved rental entity to a view model.
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
                //fetching the rental details
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
                // Convert the Rental entity to a RentModel
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
                //Directly pass this object to the view without recreating it.
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
                // Create an instance of the RentDAL
                RentDAL rentDAL = new RentDAL();
                //using a mapping function to convert between models
                Rental rental = ConvertToRental(rentModel);
                rental.ReturnOdoReading = rentModel.ReturnOdoReading;
                // Update the rental details
                rentDAL.ReturnCar(id, rental);
                return RedirectToAction("Payment", new {id=id});
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        // Sample conversion function RentModel to a Rental entity.
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
                // Calculate charges based on the rental details.
                Tuple<int, double> charges = rd.CalculateCharges(rental);
                costDetails.KmsCovered = charges.Item1;

                // Update customer loyalty points based on kilometers covered.
                int customerId = Convert.ToInt32(rental.CustomerID);
                csdal.AddLoyalty(costDetails.KmsCovered, customerId);

                // Convert the charge to decimal for further calculations.
                decimal charge = Convert.ToDecimal(charges.Item2);
                decimal tax = 0M;

                // Calculate tax based on the value of charge.
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
            // Fetching rental details based on given ID
            Rental rental = rd.FindRent(id);
            if (rental == null)
            {
                return RedirectToAction("Error", new { message = "Rental not found." });
            }
            int customerId = Convert.ToInt32(rental.CustomerID);
            // Fetching customer details based on customer ID from rental
            Customer customer = csdal.GetCustomer(customerId);
            // Check if customer is eligible for a discount based on loyalty points
            if (customer.LoyaltyPoints >= 10)
            {
                const decimal DiscountRate = 0.1M;  // Using a constant for discount rate
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