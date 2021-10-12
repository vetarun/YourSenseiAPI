using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Subscription;

using Org.BouncyCastle.Crypto.Tls;
using System.Web.Configuration;

namespace YourSensei.Service
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly IEmailWorkQueueService _emailWorkQueueService;
        public SubscriptionService(YourSensei_DBEntities context, EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _emailWorkQueueService = emailWorkQueueService;

        }

        public async Task<List<SubscriptionPlanViewModel>> GetSubscriptionPlans(int id)
        {
            try
            {
                List<SubscriptionPlanViewModel> subscriptionPlanViewModels = await (from sp in _context.SubscriptionPlans
                                                                                    where sp.IsActive == true
                                                                                      && (sp.ID == id || id == 0)
                                                                                    select new SubscriptionPlanViewModel
                                                                                    {
                                                                                        ID = sp.ID,
                                                                                        PlanName = sp.Name,
                                                                                        Description = sp.Description,
                                                                                        NumberOfDays = sp.NumberOfDays,
                                                                                        NumberOfEmployees = sp.NumberOfEmployees,
                                                                                        NumberOfExternalMentors = sp.NumberOfExternalMentors,
                                                                                        Price = sp.Price,
                                                                                        FeaturesAllowed = sp.FeaturesAllowed,
                                                                                        IsActive = sp.IsActive,
                                                                                        IsTrialPlan = sp.IsTrialPlan
                                                                                    }).ToListAsync();

                foreach (SubscriptionPlanViewModel subscriptionPlanViewModel in subscriptionPlanViewModels)
                {
                    subscriptionPlanViewModel.FeaturesAllowedList = new List<string>();
                    subscriptionPlanViewModel.FeaturesAllowedArray = subscriptionPlanViewModel.FeaturesAllowed.
                        Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    string[] names = Enum.GetNames(typeof(FeaturesAllowed));
                    foreach (string name in names)
                    {
                        if (subscriptionPlanViewModel.FeaturesAllowed.Contains("," + name + ","))
                        {
                            string enumDescription = EnumHelper.GetDescription((FeaturesAllowed)Enum.Parse(typeof(FeaturesAllowed), name));
                            subscriptionPlanViewModel.FeaturesAllowedList.Add(enumDescription);
                            if (string.IsNullOrWhiteSpace(subscriptionPlanViewModel.FinalFeaturesAllowed))
                                subscriptionPlanViewModel.FinalFeaturesAllowed = enumDescription;
                            else
                                subscriptionPlanViewModel.FinalFeaturesAllowed = subscriptionPlanViewModel.FinalFeaturesAllowed + ", " + enumDescription;
                        }
                    }
                }

                return subscriptionPlanViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SubscriptionResponseViewModel> SubscribePlanToUser(string userdetailid, string companyid, int planID, int noOfDays)
        {
            try
            {
                var date = DateTime.UtcNow;
                bool isActivated = true;
                int daysLeft = 0;
                Subscription subscription;
                if (!String.IsNullOrWhiteSpace(companyid))
                    subscription = await _context.Subscriptions.FirstOrDefaultAsync(x => x.CompanyID == new Guid(companyid) && x.IsExpired == false && 
                        x.IsActivated == true && x.IsActivated == true);
                else
                    subscription = await _context.Subscriptions.FirstOrDefaultAsync(x => x.UserDetailID == new Guid(userdetailid) && x.IsExpired == false && 
                        x.IsActivated == true);

                if (subscription != null)
                {
                    if (subscription.PlanID == planID)
                    {
                        TimeSpan difference = Convert.ToDateTime(subscription.ExpiryDate).Date - date.Date;
                        daysLeft = (int)difference.TotalDays;

                        if(daysLeft < 0)
                        {
                            subscription.IsActivated = false;
                            subscription.IsExpired = true;
                            subscription.ModifiedBy = new Guid(userdetailid);
                            subscription.ModifiedDate = date;
                            await _context.SaveChangesAsync();
                        }
                        isActivated = subscription.IsExpired ? true : false;

                    }
                    else
                    {
                        subscription.IsActivated = false;
                        subscription.IsExpired = true;
                        subscription.ModifiedBy = new Guid(userdetailid);
                        subscription.ModifiedDate = date;
                        await _context.SaveChangesAsync();
                    }

                }

                SubscriptionPlan subscriptionPlan = _context.SubscriptionPlans.Where(a => a.ID == planID).FirstOrDefault();
                Employee employee = (from e in _context.Employees
                                     join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                     where ud.ID == new Guid(userdetailid) && e.IsActive == true
                                     select e).FirstOrDefault();

                Subscription assignPlan = new Subscription();
                assignPlan.PlanID = planID;
                if (!String.IsNullOrWhiteSpace(companyid))
                    assignPlan.CompanyID = new Guid(companyid);
                else
                    assignPlan.UserDetailID = new Guid(userdetailid);
                assignPlan.PurchasedDate = date;
                assignPlan.PurchasedBy = new Guid(userdetailid);
                assignPlan.ActivationDate = date;
                assignPlan.ActivatedBy = new Guid(userdetailid);
                assignPlan.RenewalDate = null;
                assignPlan.RenewedBy = null;
                assignPlan.ExpiryDate = date.AddDays(noOfDays);
                assignPlan.IsExpired = false;
                assignPlan.CreatedBy = new Guid(userdetailid);
                assignPlan.CreateDate = date;
                assignPlan.ModifiedBy = new Guid(userdetailid);
                assignPlan.ModifiedDate = date;
                assignPlan.IsActivated = isActivated;


                if (isActivated)
                {
                    _context.Subscriptions.Add(assignPlan);
                    await _context.SaveChangesAsync();
                    EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                    {
                        WorkItemType = "SubscribePlan",
                        KeyID = planID.ToString(),
                        KeyType = "",
                        SendToEmployee = new Guid(userdetailid),
                        Subject = "Plan Subscription Confirmation",
                        Body = "",
                        Template = "SubscribePlan.html",
                        TemplateContent = "",
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await _emailWorkQueueService.Save(emailWorkQueue);
                    //SendMail.SendEmailToPlanSubscriber(new SubscriberEmailInputModel
                    //{
                    //    SubscriberEmail = employee.Email,
                    //    SubscriberName = employee.FirstName + " " + employee.LastName,
                    //    PlanName = subscriptionPlan.Name,
                    //    ActivationDate = assignPlan.ActivationDate,
                    //    AccessableFeatures = subscriptionPlan.FeaturesAllowed,
                    //    TotalNumberOfEmployees = subscriptionPlan.NumberOfEmployees,
                    //    TotalNumberOfMentor = subscriptionPlan.NumberOfExternalMentors,
                    //    TotalNumberOfPlanDays = subscriptionPlan.NumberOfDays,
                    //    ExpiryDate = assignPlan.ExpiryDate.GetValueOrDefault()
                    //});
                }
                else
                {
                    assignPlan.IsActivated = true;
                    assignPlan.ActivationDate = date.AddDays(daysLeft);
                    assignPlan.RenewalDate = date;
                    assignPlan.RenewedBy = new Guid(userdetailid);
                    assignPlan.ExpiryDate = date.AddDays(daysLeft + noOfDays);

                    WorkQueue queue = new WorkQueue()
                    {
                        CompanyID = new Guid(companyid),
                        WorkData = new JavaScriptSerializer().Serialize(assignPlan),                       
                        Status = "Pending",
                        WorkItemType = "RenewSubscription",
                        CreatedDate = date,
                        UserDetailID = new Guid(userdetailid)
                    };
                    _context.WorkQueues.Add(queue);
                    _context.SaveChanges();
                    EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                    {
                        WorkItemType = "RenewPlan",
                        KeyID = planID.ToString(),
                        KeyType = "",
                        SendToEmployee = new Guid(userdetailid),
                        Subject = "Plan Subscription Confirmation",
                        Body = "",
                        Template = "RenewPlan.html",
                        TemplateContent = "",
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await _emailWorkQueueService.Save(emailWorkQueue);
                    //SendMail.SendEmailToRenewPlan(new SubscriberEmailInputModel
                    //{
                    //    SubscriberEmail = employee.Email,
                    //    SubscriberName = employee.FirstName + " " + employee.LastName,
                    //    PlanName = subscriptionPlan.Name,
                    //    ActivationDate = assignPlan.ActivationDate,
                    //    AccessableFeatures = subscriptionPlan.FeaturesAllowed,
                    //    TotalNumberOfEmployees = subscriptionPlan.NumberOfEmployees,
                    //    TotalNumberOfMentor = subscriptionPlan.NumberOfExternalMentors,
                    //    TotalNumberOfPlanDays = subscriptionPlan.NumberOfDays,
                    //    ExpiryDate = assignPlan.ExpiryDate.GetValueOrDefault()
                    //});
                }

                return new SubscriptionResponseViewModel { Code = 200, Message = "Successfully Subscribed",SubscriptionID= assignPlan.ID };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<List<SubscriptionViewModel>> GetSubscriptions()
        {
            try
            {
                List<SubscriptionViewModel> subscriptionViewModels = await _context.Database.SqlQuery<SubscriptionViewModel>(
                    "dbo.usp_GetSubscriptions").ToListAsync();
                return subscriptionViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> addUpdateSubscriptionPlan(SubscriptionPlanViewModel obj)
        {
            try
            {
                if (obj.ID != 0)
                {
                    var result = await _context.SubscriptionPlans.FirstOrDefaultAsync(d => d.ID == obj.ID);

                    result.Name = obj.PlanName;
                    result.Description = obj.Description;
                    result.NumberOfDays = obj.NumberOfDays;
                    result.NumberOfEmployees = obj.NumberOfEmployees;
                    result.NumberOfExternalMentors = obj.NumberOfExternalMentors;
                    result.Price = obj.Price;
                    ///result.IsTrialPlan = obj.IsTrialPlan;
                    result.FeaturesAllowed = obj.FeaturesAllowed;
                    result.ModifiedBy = new Guid(obj.ModifiedBy);
                    result.ModifiedDate = DateTime.UtcNow;
                    result.IsActive = obj.IsActive;
                    if (!obj.IsActive)
                    {
                        manageSubscriptionHistory("delete", result);
                    }
                    else if (obj.IsActive)
                    {
                        manageSubscriptionHistory("update", result);
                    }
                    _context.SaveChanges();

                    return new ResponseViewModel { Code = 200, Message = "Data has been updated successfully!" };
                }
                else
                {
                    SubscriptionPlan result = new SubscriptionPlan();
                    result.Name = obj.PlanName;
                    result.Description = obj.Description;
                    result.NumberOfDays = obj.NumberOfDays;
                    result.NumberOfEmployees = obj.NumberOfEmployees;
                    result.NumberOfExternalMentors = obj.NumberOfExternalMentors;
                    result.Price = obj.Price;
                    result.IsTrialPlan = obj.IsTrialPlan;
                    result.FeaturesAllowed = obj.FeaturesAllowed;
                    result.ModifiedBy = new Guid(obj.ModifiedBy);
                    result.ModifiedDate = DateTime.UtcNow;
                    result.CreatedBy = new Guid(obj.CreatedBy);
                    result.CreateDate = DateTime.UtcNow;
                    result.IsActive = true;

                    _context.SubscriptionPlans.Add(result);
                    _context.SaveChanges();
                    manageSubscriptionHistory("add", result);
                    return new ResponseViewModel { Code = 200, Message = "Data has been saved successfully!" };
                }


            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = ex.Message.ToString() };
            }


        }

        public async Task<FeaturesModel> GetFeaturesAllowed()
        {
            FeaturesModel rtndata = new FeaturesModel();
            List<FeaturesAllowedModel> result = new List<FeaturesAllowedModel>();
            result = await EnumHelper.GetDescriptions(typeof(FeaturesAllowed));
            rtndata.FeaturesAllowed = result;
            rtndata.IsTrailPlanAlreadyExist = _context.SubscriptionPlans.Any(d => d.IsTrialPlan && d.IsActive == true);
            return rtndata;
        }

        public async Task<SubscriptionPlan> GetSubscribedPlan(Guid companyID, Guid userDetailID)
        {
            try
            {
                SubscriptionPlan subscriptionPlan = new SubscriptionPlan();
                Subscription subscription = null;
                if (companyID == Guid.Empty && userDetailID != Guid.Empty)
                    subscription = await _context.Subscriptions.Where(a => a.UserDetailID == userDetailID && a.IsExpired == false && 
                        a.IsActivated == true).FirstOrDefaultAsync();
                else if (companyID != Guid.Empty && userDetailID == Guid.Empty)
                    subscription = await _context.Subscriptions.Where(a => a.CompanyID == companyID && a.IsExpired == false && 
                        a.IsActivated == true).FirstOrDefaultAsync();

                if (subscription != null)
                {
                    List<SubscriptionPlanHistory> subscriptionPlanHistories = _context.SubscriptionPlanHistories.
                        Where(a => a.PlanID == subscription.PlanID).OrderByDescending(a => a.ModifiedDate).ToList();

                    bool isPlanExists = false;
                    foreach (SubscriptionPlanHistory subscriptionPlanHistory in subscriptionPlanHistories)
                    {
                        subscriptionPlan = new JavaScriptSerializer().Deserialize<SubscriptionPlan>(subscriptionPlanHistory.PlanObject);
                        if (subscriptionPlan.ModifiedDate <= subscription.PurchasedDate)
                        {
                            isPlanExists = true;
                            break;
                        }
                    }

                    if (!isPlanExists)
                    {
                        subscriptionPlan = _context.SubscriptionPlans.Where(a => a.ID == subscription.PlanID).FirstOrDefault();
                    }
                }
                else
                {
                    if (companyID == Guid.Empty && userDetailID != Guid.Empty)
                        subscription = await _context.Subscriptions.Where(a => a.UserDetailID == userDetailID).OrderByDescending(a=>a.ModifiedDate).
                            FirstOrDefaultAsync();
                    else if (companyID != Guid.Empty && userDetailID == Guid.Empty)
                        subscription = await _context.Subscriptions.Where(a => a.CompanyID == companyID).OrderByDescending(a => a.ModifiedDate).
                            FirstOrDefaultAsync();

                    List<SubscriptionPlanHistory> subscriptionPlanHistories = _context.SubscriptionPlanHistories.
                        Where(a => a.PlanID == subscription.PlanID).OrderByDescending(a => a.ModifiedDate).ToList();

                    bool isPlanExists = false;
                    foreach (SubscriptionPlanHistory subscriptionPlanHistory in subscriptionPlanHistories)
                    {
                        subscriptionPlan = new JavaScriptSerializer().Deserialize<SubscriptionPlan>(subscriptionPlanHistory.PlanObject);
                        if (subscriptionPlan.ModifiedDate <= subscription.PurchasedDate)
                        {
                            isPlanExists = true;
                            break;
                        }
                    }

                    if (!isPlanExists)
                    {
                        subscriptionPlan = _context.SubscriptionPlans.Where(a => a.ID == subscription.PlanID).FirstOrDefault();
                    }
                }
                return subscriptionPlan;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void manageSubscriptionHistory(string mode, SubscriptionPlan inputobj)
        {
            SubscriptionPlanHistory obj = new SubscriptionPlanHistory();
            obj.CreatedBy = inputobj.CreatedBy;
            obj.CreatedDate = DateTime.UtcNow;
            obj.ModififedBy = inputobj.ModifiedBy;
            obj.PlanID = inputobj.ID;
            obj.ModifiedDate = DateTime.UtcNow;
            obj.PlanObject = new JavaScriptSerializer().Serialize(inputobj);
            obj.Action = mode;
            _context.SubscriptionPlanHistories.Add(obj);
            _context.SaveChanges();
        }

        public async Task<Subscription> GetSubscriptionsByCompanyIDAndPlanID(Guid? companyID, int planID)
        {
            try
            {
                return await _context.Subscriptions.Where(a => a.CompanyID == companyID && a.PlanID == planID && a.IsActivated == true && 
                    a.IsExpired == false).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> Save(Subscription subscription)
        {
            try
            {
                if (subscription != null)
                {
                    if (subscription.ID == 0)
                        _context.Entry(subscription).State = EntityState.Added;
                    else
                        _context.Entry(subscription).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return new ResponseViewModel { Code = 200, Message = "Successfully saved" };
                }

                return new ResponseViewModel { Code = 400, Message = "Not found" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Subscription> GetSubscriptionsByUserDetailIDAndPlanID(Guid? userDetailID, int planID)
        {
            try
            {
                return await _context.Subscriptions.Where(a => a.UserDetailID == userDetailID && a.PlanID == planID && a.IsActivated == true &&
                    a.IsExpired == false).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Subscription>> GetSubscriptionsByIsActivatedAndIsExpired(bool isActivated, bool isExpired)
        {
            try
            {
                return await _context.Subscriptions.Where(a => a.IsActivated == isActivated && a.IsExpired == isExpired).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ResponseViewModel SavePayPalTransaction(SubscriptionTransaction obj)
        {
            try
            {
                SubscriptionTransaction inpObj = new SubscriptionTransaction()
                {
                    PlanID = obj.PlanID,
                    SubscriptionID = obj.SubscriptionID,
                    Payment = obj.Payment,
                    TransactionDate = DateTime.UtcNow,
                    TransactionStatus = obj.TransactionStatus,
                    TransactionID = obj.TransactionID,
                    TransactionMode = obj.TransactionMode,
                    PaymentCardDetailID = obj.PaymentCardDetailID
                };
                
                _context.SubscriptionTransactions.Add(inpObj);
                int r= _context.SaveChanges();

                if (r > 0)
                {
                    return new ResponseViewModel { Code = 200, Message = "Transaction completed successfully!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 201, Message = "Transaction completed successfully!" };
                }
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 201, Message = ex.Message };
            }
        }
    }
}
