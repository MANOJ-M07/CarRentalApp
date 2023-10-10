using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    public class CarDAL
    {
        CarRentalEntities context = null;

        public CarDAL()
        {
            context = new CarRentalEntities();
        }

        public List<Car> getcar()
        {
            return context.Cars.ToList();
        }
        public bool addcar(Car c)
        {
            try
            {
                context.Cars.Add(c);
                context.SaveChanges();
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // This will give detailed validation errors
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        // Log these details, this will give you an insight into which property (or properties) are causing the validation error.
                        System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                return false; // or rethrow, depending on your error handling strategy
            }
            catch (Exception ex)
            {
                // Handle other exceptions as necessary
                throw new ApplicationException("Unable to add car.", ex);
            }
        }


        /*public bool addcar(Car c)
        {
            try
            {
                context.Cars.Add(c);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as necessary
                throw new ApplicationException("Unable to add car.", ex);
            }
        }*/

        public Car find(int id)
        {
            return context.Cars.FirstOrDefault(x => x.CarID == id);
        }

        public bool delete(int id)
        {
            try
            {
                Car carToDelete = context.Cars.FirstOrDefault(x => x.CarID == id);
                if (carToDelete == null)
                {
                    throw new ArgumentException("Car with the specified ID not found.");
                }

                // Delete all associated rentals first
                var rentalsToDelete = context.Rentals.Where(r => r.CarID == id).ToList();
                foreach (var rental in rentalsToDelete)
                {
                    context.Rentals.Remove(rental);
                }

                // Now delete the car
                context.Cars.Remove(carToDelete);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to delete car.", ex);
            }
        }


        /* public bool delete(int id)
         {
             try
             {
                 Car carToDelete = context.Cars.FirstOrDefault(x => x.CarID == id);
                 if (carToDelete == null)
                 {
                     throw new ArgumentException("Car with the specified ID not found.");
                 }

                 context.Cars.Remove(carToDelete);
                 context.SaveChanges();
                 return true;
             }
             catch (Exception ex)
             {
                 throw new ApplicationException("Unable to delete car.", ex);
             }
         }*/

        public bool update(int id, Car c)
        {
            try
            {


                Car carToUpdate = context.Cars.Find(id);
                if (carToUpdate == null)
                {
                    return false;
                    //throw new ArgumentException("Car with the specified ID not found.");
                }

                carToUpdate.CarName = c.CarName;
                carToUpdate.PerDayCharge = c.PerDayCharge;
                carToUpdate.ChargePerKm = c.ChargePerKm;
                carToUpdate.Photo = c.Photo;
                carToUpdate.CarType = c.CarType;
                carToUpdate.Available = c.Available;

                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
               // throw new ApplicationException("Unable to update car.", ex);
            }
        }

        public void locked(int id)
        {
            Car carToLock = context.Cars.Find(id);
            if (carToLock == null)
            {
                throw new ArgumentException("Car with the specified ID not found.");
            }

            carToLock.Available = false;  // Here, set the boolean to false for "No"
            context.SaveChanges();
        }

        public void unlocked(int id)
        {
            Car carToUnlock = context.Cars.Find(id);
            if (carToUnlock == null)
            {
                throw new ArgumentException("Car with the specified ID not found.");
            }

            carToUnlock.Available = true;  // Here, set the boolean to true for "Yes"
            context.SaveChanges();
        }



    }
}
