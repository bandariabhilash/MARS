using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;
using System.Globalization;
using NewcoCall.Models;
using NewcoCall;

namespace FBCall.Models
{
    public class WorkorderManagementModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
                                ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            WorkorderManagementModel model = new WorkorderManagementModel();

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();

            model.Customer = new CustomerModel();
            model.WorkOrder = new WorkOrder();

            //model.Operation = (WorkOrderManagementSubmitType)Convert.ToInt32(request.Unvalidated.Form.Get("Operation"));

            model.SpawnNotes = request.Unvalidated.Form.Get("SpanReasonNotesHidden");
            model.SpawnReason = request.Unvalidated.Form.Get("SpawnReasonHidden");

            model.NSRNotes = request.Unvalidated.Form.Get("NSRReasonNotesHidden");
            model.NSRReason = request.Unvalidated.Form.Get("NSRReasonHidden");

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("TemperaturesHidden")))
            {
                string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("TemperaturesHidden"));

            }
            
            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("Estimate")))
            {
                model.Estimate = Convert.ToDecimal(request.Unvalidated.Form.Get("Estimate"));

            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WorkOrder.EstimateApprovedBy")))
            {
                model.WorkOrder.EstimateApprovedBy = Convert.ToInt32(request.Unvalidated.Form.Get("WorkOrder.EstimateApprovedBy"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("FinalEstimate")))
            {
                model.FinalEstimate = Convert.ToDecimal(request.Unvalidated.Form.Get("FinalEstimate"));

            }


            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("IsEstimateApproved")))
            {
                string IsEstimateApproved = request.Unvalidated.Form.Get("IsEstimateApproved");
                if (!string.IsNullOrWhiteSpace(IsEstimateApproved))
                {
                    if (IsEstimateApproved.Contains("true"))
                    {
                        model.IsEstimateApproved = true;
                    }
                }
            }

            //if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("IsBillable")))
            //{
            //    string IsBillable = request.Unvalidated.Form.Get("IsBillable");
            //    if (!string.IsNullOrWhiteSpace(IsBillable))
            //    {
            //        if (IsBillable.Contains("true"))
            //        {
            //            model.IsBillable = true;
            //        }
            //    }
            //}

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ThirdPartyPO")))
            {
                model.ThirdPartyPO = request.Unvalidated.Form.Get("ThirdPartyPO");

            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WorkOrderClosureHidden")))
                //&& (model.Operation == WorkOrderManagementSubmitType.SAVE
                //|| model.Operation == WorkOrderManagementSubmitType.COMPLETE))
            {
                IList<WorkOrderManagementEquipmentModel> woEqp = json_serializer.Deserialize<IList<WorkOrderManagementEquipmentModel>>(request.Unvalidated.Form.Get("WorkOrderClosureHidden"));


                IList<IList<WorkOrderPartModel>> woPart = new List<IList<WorkOrderPartModel>>();
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ChildGridDataHidden")))
                {
                    woPart = json_serializer.Deserialize<IList<IList<WorkOrderPartModel>>>(request.Unvalidated.Form.Get("ChildGridDataHidden"));
                }

                string[] temperatures = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("TemperaturesHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("TemperaturesHidden"));
                    temperatures = value.Split('|');
                }

                string[] settings = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("SettingsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("SettingsHidden"));
                    settings = value.Split('|');
                }

                string[] counters = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("CountersHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("CountersHidden"));
                    counters = value.Split('|');
                }

                string[] systems = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("SystemsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("SystemsHidden"));
                    systems = value.Split('|');
                }

                string[] symptoms = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("SymptomsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("SymptomsHidden"));
                    symptoms = value.Split('|');
                }

                string[] nemwNumbers = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("NemwNumberHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("NemwNumberHidden"));
                    nemwNumbers = value.Split('|');
                }

                string[] electricalPhases = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ElectricalPhaseHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("ElectricalPhaseHidden"));
                    electricalPhases = value.Split('|');
                }

                string[] machineAmperages = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("MachineAmperageHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("MachineAmperageHidden"));
                    machineAmperages = value.Split('|');
                }

                string[] unitFitSpaces = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("UnitFitSpaceHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("UnitFitSpaceHidden"));
                    unitFitSpaces = value.Split('|');
                }

                string[] voltages = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("VoltageHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("VoltageHidden"));
                    voltages = value.Split('|');
                }

                string[] counterUnitSpaces = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("CounterUnitSpaceHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("CounterUnitSpaceHidden"));
                    counterUnitSpaces = value.Split('|');
                }

                string[] waterLines = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WaterLineHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("WaterLineHidden"));
                    waterLines = value.Split('|');
                }

                string[] comments = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("CommentsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("CommentsHidden"));
                    comments = value.Split('|');
                }

                string[] assetLocations = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("LocationsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("LocationsHidden"));
                    assetLocations = value.Split('|');
                }

                string[] works = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WorksHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("WorksHidden"));
                    string workstring = value.Replace("\"", "");
                    works = value.Split('|');
                }

                string[] noParts = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("NoPartsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("NoPartsHidden"));
                    string noPartString = value.Replace("\"", "");
                    noParts = value.Split('|');
                }

                string[] qualityIssues = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("QualityIssueHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("QualityIssueHidden"));
                    qualityIssues = value.Split('|');
                }

                string[] emails = null;
                if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("EmailsHidden")))
                {
                    string value = json_serializer.Deserialize<string>(request.Unvalidated.Form.Get("EmailsHidden"));
                    emails = value.Split('|');
                }

                for (int i = 0; i < woEqp.Count; i++)
                {
                    if (temperatures != null && temperatures.Length > i)
                    {
                        woEqp[i].Temperature = temperatures[i];
                    }
                    if (settings != null && settings.Length > i)
                    {
                        woEqp[i].Settings = settings[i];
                    }
                    if (counters != null && counters.Length > i)
                    {
                        woEqp[i].Counter = counters[i];
                    }

                    if (systems != null && systems.Length > i)
                    {
                        string system = systems[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(system) && string.Compare(system, "null", 0) != 0)
                        {
                            woEqp[i].System = Convert.ToInt32(system);
                        }
                    }

                    if (symptoms != null && symptoms.Length > i)
                    {
                        string symptom = symptoms[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(symptom) && string.Compare(symptom, "null", 0) != 0)
                        {
                            woEqp[i].SymptomID = Convert.ToInt32(symptom);
                        }
                    }

                    if (nemwNumbers != null && nemwNumbers.Length > i)
                    {
                        string NemwNumber = nemwNumbers[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(NemwNumber))
                        {
                            woEqp[i].NemwNumber = NemwNumber;
                        }
                    }

                    if (nemwNumbers != null && nemwNumbers.Length > i)
                    {
                        string NemwNumber = nemwNumbers[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(NemwNumber))
                        {
                            woEqp[i].NemwNumber = NemwNumber;
                        }
                    }

                    if (electricalPhases != null && electricalPhases.Length > i)
                    {
                        string electricalPhase = electricalPhases[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(electricalPhase))
                        {
                            woEqp[i].ElectricalPhase = electricalPhase;
                        }
                    }

                    if (machineAmperages != null && machineAmperages.Length > i)
                    {
                        string machineAmperage = machineAmperages[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(machineAmperage))
                        {
                            woEqp[i].MachineAmperage = machineAmperage;
                        }
                    }

                    if (unitFitSpaces != null && unitFitSpaces.Length > i)
                    {
                        string unitFitSpace = unitFitSpaces[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(unitFitSpace))
                        {
                            woEqp[i].UnitFitSpace = unitFitSpace;
                        }
                    }

                    if (voltages != null && voltages.Length > i)
                    {
                        string voltage = voltages[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(voltage))
                        {
                            woEqp[i].Voltage = voltage;
                        }
                    }

                    if (counterUnitSpaces != null && counterUnitSpaces.Length > i)
                    {
                        string counterUnitSpace = counterUnitSpaces[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(counterUnitSpace))
                        {
                            woEqp[i].CounterUnitSpace = counterUnitSpace;
                        }
                    }

                    if (waterLines != null && waterLines.Length > i)
                    {
                        string waterLine = waterLines[i].Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(waterLine))
                        {
                            woEqp[i].WaterLine = waterLine;
                        }
                    }

                    if (comments != null && comments.Length > i)
                    {
                        woEqp[i].Comments = comments[i];
                    }

                    if (assetLocations != null && assetLocations.Length > i)
                    {
                        woEqp[i].AssetLocation = assetLocations[i];
                    }

                    if (works != null && works.Length > i)
                    {
                        woEqp[i].WorkPerformed = works[i];
                    }

                    if (noParts != null && noParts.Length > i)
                    {
                        if (string.IsNullOrEmpty(noParts[i]))
                        {
                            woEqp[i].NoPartsNeeded = false;
                        }
                        else
                        {
                            woEqp[i].NoPartsNeeded = Convert.ToBoolean(noParts[i]);
                        }
                    }

                    bool QI = false;
                    if (qualityIssues != null && qualityIssues.Length > i)
                    {
                        bool.TryParse(qualityIssues[i].Replace("\"", ""), out QI);
                    }
                    woEqp[i].QualityIssue = QI;

                    if (emails != null && emails.Length > i)
                    {
                        woEqp[i].Email = emails[i];
                    }

                    if (woPart.Count > 0 && woPart.Count > i)
                    {
                        woEqp[i].Parts = woPart[i];
                    }
                }

                WorkOrderClosureModel woClosure = new WorkOrderClosureModel();
                {
                    string startDate = request.Unvalidated.Form.Get("Closure.StartDateTime");
                    string arrivalDate = request.Unvalidated.Form.Get("Closure.ArrivalDateTime");
                    string completionDate = request.Unvalidated.Form.Get("Closure.CompletionDateTime");

                    if (!string.IsNullOrWhiteSpace(startDate) && !(string.Compare(startDate, "Invalid Date", true) == 0))
                    {
                        woClosure.StartDateTime = Convert.ToDateTime(startDate);
                    }

                    if (!string.IsNullOrWhiteSpace(arrivalDate) && !(string.Compare(arrivalDate, "Invalid Date", true) == 0))
                    {
                        woClosure.ArrivalDateTime = Convert.ToDateTime(arrivalDate);
                    }

                    if (!string.IsNullOrWhiteSpace(completionDate) && !(string.Compare(completionDate, "Invalid Date", true) == 0))
                    {
                        woClosure.CompletionDateTime = Convert.ToDateTime(completionDate);
                    }

                    woClosure.InvoiceNo = request.Unvalidated.Form.Get("Closure.InvoiceNo");
                    woClosure.CustomerEmail = request.Unvalidated.Form.Get("Closure.CustomerEmail");
                    woClosure.CustomerName = request.Unvalidated.Form.Get("Closure.CustomerName");

                    if (request.Unvalidated.Form.Get("NewWorkorderCallstatusHidden") != "Closed")
                    {
                        woClosure.CustomerSignatureDetails = request.Unvalidated.Form.Get("CustomerSignatureDetailsHidden");
                    }

                    decimal Mlg = 0;
                    decimal.TryParse(request.Unvalidated.Form.Get("Closure.Mileage"), out Mlg);
                    woClosure.Mileage = Mlg;

                    //if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("Closure.PhoneSolveid")))
                    //{
                    //    woClosure.PhoneSolveid = Convert.ToInt32(request.Unvalidated.Form.Get("Closure.PhoneSolveid"));
                    //}

                    woClosure.ResponsibleTechName = request.Unvalidated.Form.Get("Closure.ResponsibleTechName");
                    woClosure.SpecialClosure = request.Unvalidated.Form.Get("Closure.SpecialClosure");
                    woClosure.TravelHours = request.Unvalidated.Form.Get("Closure.TravelHours");
                    woClosure.TravelMinutes = request.Unvalidated.Form.Get("Closure.TravelMinutes");
                    woClosure.WorkOrderEquipments = woEqp;
                }

                model.Closure = woClosure;
                model.WorkOrderEquipments = woEqp;
            }
            else
            {
                model.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                model.Closure = new WorkOrderClosureModel();
                model.Closure.WorkOrderEquipments = new List<WorkOrderManagementEquipmentModel>();
                model.Closure.SpecialClosure = request.Unvalidated.Form.Get("Closure.SpecialClosure");
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("PhoneSolveId")))
            {
                model.PhoneSolveId = Convert.ToInt32(request.Unvalidated.Form.Get("PhoneSolveId"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("PhoneSolveTechId")))
            {
                model.PhoneSolveTechId = Convert.ToInt32(request.Unvalidated.Form.Get("PhoneSolveTechId"));
            }

            foreach (var property in model.Customer.GetType().GetProperties())
            {
                property.SetValue(model.Customer, request.Unvalidated.Form.Get(property.Name));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("Customer.Zipcode")))
            {
                model.Customer.ZipCode = request.Unvalidated.Form.Get("Customer.Zipcode");
                model.customerZipcode = model.Customer.ZipCode;
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("Zipcode")))
            {
                model.customerZipcode = Convert.ToString(request.Unvalidated.Form.Get("Zipcode"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ServiceTier")))
            {
                model.Customer.ServiceTier = request.Unvalidated.Form.Get("ServiceTier");
            }

            foreach (var property in model.WorkOrder.GetType().GetProperties())
            {
                string value = request.Unvalidated.Form.Get("WorkOrder." + property.Name);

                if (!string.IsNullOrWhiteSpace(value) && !(string.Compare(value, "Invalid Date", true) == 0))
                {
                    if (property.PropertyType == typeof(Int32))
                    {
                        property.SetValue(model.WorkOrder, Convert.ToInt32(value));
                    }
                    else if (property.PropertyType == typeof(Nullable<int>))
                    {
                        property.SetValue(model.WorkOrder, new Nullable<int>(Convert.ToInt32(value)));
                    }
                    else if (property.PropertyType == typeof(DateTime)
                        || property.PropertyType == typeof(Nullable<DateTime>))
                    {
                        //var date = DateTime.ParseExact(value, "M/d/yyyy", new CultureInfo("en-US", true), DateTimeStyles.None);
                        var date = Convert.ToDateTime(value);
                        //date = DateTime.ParseExact(value, "M/d/yyyy HH:mm tt", new CultureInfo("en-US", true));
                        property.SetValue(model.WorkOrder, date);
                    }
                    else if (property.PropertyType == typeof(Nullable<bool>)
                        || property.PropertyType == typeof(bool))
                    {
                        property.SetValue(model.WorkOrder.PartsRushOrder, Convert.ToBoolean(value));
                    }
                    else
                    {
                        property.SetValue(model.WorkOrder, value);
                    }
                }
            }

            model.WorkOrder.WorkorderCallstatus = request.Unvalidated.Form.Get("NewWorkorderCallstatusHidden");

            string rushOrder = request.Unvalidated.Form.Get("WorkOrder.PartsRushOrder.Value");
            if (!string.IsNullOrWhiteSpace(rushOrder))
            {
                if (rushOrder.Contains("true"))
                {
                    model.WorkOrder.PartsRushOrder = true;
                }
            }

            model.SelectedBrandIds = request.Unvalidated.Form.Get("SelectedBrandIds");
            model.SalesNotificationCode = request.Unvalidated.Form.Get("SalesNotificationCode");
            model.SalesNotificationNotes = request.Unvalidated.Form.Get("SalesNotificationNotes");
            model.IsNewPartsOrder = Convert.ToBoolean(request.Unvalidated.Form.Get("IsNewPartsOrder"));
            model.OverTimeRequestDescription = request.Unvalidated.Form.Get("OverTimeRequestDescription");

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("RowIdHidden")))
            {
                model.RowId = Convert.ToInt32(request.Unvalidated.Form.Get("RowIdHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("RemovalCountHidden")))
            {
                model.RemovalCount = Convert.ToInt32(request.Unvalidated.Form.Get("RemovalCountHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("RemovalDateHidden")))
            {
                model.RemovalDate = DateTime.ParseExact(request.Unvalidated.Form.Get("RemovalDateHidden"), "MM/dd/yyyy",
                                      CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("RemovaAllHidden")))
            {
                model.RemovaAll = Convert.ToBoolean(request.Unvalidated.Form.Get("RemovaAllHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("AppointmentReasonHidden")))
            {
                model.AppointmentUpdateReason = Convert.ToInt32(request.Unvalidated.Form.Get("AppointmentReasonHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("RemovalReasonHidden")))
            {
                model.RemovalReason = Convert.ToInt32(request.Unvalidated.Form.Get("RemovalReasonHidden"));
            }

            model.BeveragesSupplier = request.Unvalidated.Form.Get("BeveragesSupplierHidden");

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ClosingBusinessHidden")))
            {
                model.ClosingBusiness = Convert.ToBoolean(request.Unvalidated.Form.Get("ClosingBusinessHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("FlavorOrTasteOfCoffeeHidden")))
            {
                model.FlavorOrTasteOfCoffee = Convert.ToBoolean(request.Unvalidated.Form.Get("FlavorOrTasteOfCoffeeHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("EquipmentServiceReliabilityorResponseTimeHidden")))
            {
                model.EquipmentServiceReliabilityorResponseTime = Convert.ToBoolean(request.Unvalidated.Form.Get("EquipmentServiceReliabilityorResponseTimeHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("CostPerCupHidden")))
            {
                model.CostPerCup = Convert.ToBoolean(request.Unvalidated.Form.Get("CostPerCupHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ChangingGroupPurchasingProgramHidden")))
            {
                model.ChangingGroupPurchasingProgram = Convert.ToBoolean(request.Unvalidated.Form.Get("ChangingGroupPurchasingProgramHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ChangingDistributorHidden")))
            {
                model.ChangingDistributor = Convert.ToBoolean(request.Unvalidated.Form.Get("ChangingDistributorHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("RowIdHidden")))
            {
                model.RowId = Convert.ToInt32(request.Unvalidated.Form.Get("RowIdHidden"));
            }

            model.Notes = new NotesModel();
            model.Notes.Notes = request.Unvalidated.Form.Get("Notes");
            model.Notes.FollowUpRequestID = request.Unvalidated.Form.Get("FollowUpRequestID");

            string isSpecificCheckValue = "false";
            if (request.Unvalidated.Form.Get("IsSpecificTechnician") != null)
                isSpecificCheckValue = (request.Unvalidated.Form.Get("IsSpecificTechnician").Split(','))[0];
            if (isSpecificCheckValue.Contains("true"))
                model.Notes.IsSpecificTechnician = true;
            else
                model.Notes.IsSpecificTechnician = false;

            string IsAutoDispatchedCheckValue = "false";
            if (request.Unvalidated.Form.Get("Notes.IsAutoDispatched") != null)
                IsAutoDispatchedCheckValue = (request.Unvalidated.Form.Get("Notes.IsAutoDispatched").Split(','))[0];

            model.Notes.IsAutoDispatched = IsAutoDispatchedCheckValue.Contains("true") ? true : false;

            if (!model.Notes.IsAutoDispatched)
            {
                if (request.Unvalidated.Form.Get("IsAutoDispatched") != null)
                    IsAutoDispatchedCheckValue = (request.Unvalidated.Form.Get("IsAutoDispatched").Split(','))[0];

                model.Notes.IsAutoDispatched = IsAutoDispatchedCheckValue.Contains("true") ? true : false;
            }


            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("PreferredProvider")))
            {
                model.Notes.TechID = request.Unvalidated.Form.Get("PreferredProvider");
            }


            model.IsBranchAlternateAddress = false;
            model.IsCustomerAlternateAddress = false;

            string partsShipTo = request.Unvalidated.Form.Get("PartsShipTo");
            if (!string.IsNullOrWhiteSpace(partsShipTo))
            {
                model.PartsShipTo = Convert.ToInt32(request.Unvalidated.Form.Get("PartsShipTo"));

                string isAlternateAddress = string.Empty;
                switch (model.PartsShipTo)
                {
                    case 1:
                        isAlternateAddress = request.Unvalidated.Form.Get("IsBranchAlternateAddress");
                        if (!string.IsNullOrWhiteSpace(isAlternateAddress))
                        {
                            if (isAlternateAddress.Contains("true"))
                            {
                                model.IsBranchAlternateAddress = true;

                                model.BranchOtherPartsName = request.Unvalidated.Form.Get("BranchOtherPartsName");
                                model.BranchOtherPartsContactName = request.Unvalidated.Form.Get("BranchOtherPartsContactName");
                                model.BranchOtherPartsAddress1 = request.Unvalidated.Form.Get("BranchOtherPartsAddress1");
                                model.BranchOtherPartsAddress2 = request.Unvalidated.Form.Get("BranchOtherPartsAddress2");
                                model.BranchOtherPartsCity = request.Unvalidated.Form.Get("BranchOtherPartsCity");
                                model.BranchOtherPartsState = request.Unvalidated.Form.Get("BranchOtherPartsState");
                                model.BranchOtherPartsZip = request.Unvalidated.Form.Get("BranchOtherPartsZip");
                                model.BranchOtherPartsPhone = request.Unvalidated.Form.Get("BranchOtherPartsPhone");
                            }
                        }
                        break;
                    case 2:
                        isAlternateAddress = request.Unvalidated.Form.Get("IsCustomerAlternateAddress");
                        if (!string.IsNullOrWhiteSpace(isAlternateAddress))
                        {
                            if (isAlternateAddress.Contains("true"))
                            {
                                model.IsCustomerAlternateAddress = true;
                                model.CustomerOtherPartsName = request.Unvalidated.Form.Get("CustomerOtherPartsName");
                                model.CustomerOtherPartsContactName = request.Unvalidated.Form.Get("CustomerOtherPartsContactName");
                                model.CustomerOtherPartsAddress1 = request.Unvalidated.Form.Get("CustomerOtherPartsAddress1");
                                model.CustomerOtherPartsAddress2 = request.Unvalidated.Form.Get("CustomerOtherPartsAddress2");
                                model.CustomerOtherPartsCity = request.Unvalidated.Form.Get("CustomerOtherPartsCity");
                                model.CustomerOtherPartsState = request.Unvalidated.Form.Get("CustomerOtherPartsState");
                                model.CustomerOtherPartsZip = request.Unvalidated.Form.Get("CustomerOtherPartsZip");
                                model.CustomerOtherPartsPhone = request.Unvalidated.Form.Get("CustomerOtherPartsPhone");
                            }
                        }
                        break;
                }
            }

            string projectFlatRate = request.Unvalidated.Form.Get("ProjectFlatRate");
            if (!string.IsNullOrWhiteSpace(projectFlatRate))
            {
                model.Notes.ProjectFlatRate = Convert.ToDecimal(projectFlatRate);
            }

            string projectNumber = request.Unvalidated.Form.Get("ProjectNumber");
            if (!string.IsNullOrWhiteSpace(projectNumber))
            {
                model.Notes.ProjectNumber = Convert.ToInt32(projectNumber);
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("EquipmentDetailsHidden")))
            {
                model.WorkOrderEquipmentsRequested = json_serializer.Deserialize<IList<WorkOrderManagementEquipmentModel>>(request.Unvalidated.Form.Get("EquipmentDetailsHidden"));
            }
            else
            {
                model.WorkOrderEquipmentsRequested = new List<WorkOrderManagementEquipmentModel>();
            }


            //if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("FBBillableDetailsHidden")))
            //{
            //    model.BillableSKUList = json_serializer.Deserialize<IList<FbWorkorderBillableSKUModel>>(request.Unvalidated.Form.Get("FBBillableDetailsHidden"));
            //}
            //else
            //{
            //    model.BillableSKUList = new List<FbWorkorderBillableSKUModel>();
            //}

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("NonSerializedDetailsHidden")))
            {
                model.NonSerializedList = json_serializer.Deserialize<IList<WorkOrderManagementNonSerializedModel>>(request.Unvalidated.Form.Get("NonSerializedDetailsHidden"));
            }
            else
            {
                model.NonSerializedList = new List<WorkOrderManagementNonSerializedModel>();
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WorkOrderPartsHidden")))
            {
                model.WorkOrderParts = json_serializer.Deserialize<IList<WorkOrderPartModel>>(request.Unvalidated.Form.Get("WorkOrderPartsHidden"));
            }
            else
            {
                model.WorkOrderParts = new List<WorkOrderPartModel>();
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WorkOrderNotesHidden")))
            {
                model.NewNotes = json_serializer.Deserialize<IList<NewNotesModel>>(request.Unvalidated.Form.Get("WorkOrderNotesHidden"));
            }
            else
            {
                model.NewNotes = new List<NewNotesModel>();
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ResponsibleTechIdsHidden")))
            {
                IList<int> responsibleTechIds = json_serializer.Deserialize<IList<int>>(request.Unvalidated.Form.Get("ResponsibleTechIdsHidden"));
                if (responsibleTechIds != null && responsibleTechIds.Count > 0)
                {
                    model.ResponsibleTechId = responsibleTechIds[0];
                }
                else
                {
                    model.ResponsibleTechId = new Nullable<int>();
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("AssistTechIdsHidden")))
            {
                model.AssistTechIds = json_serializer.Deserialize<IList<int>>(request.Unvalidated.Form.Get("AssistTechIdsHidden"));
            }
            else
            {
                model.AssistTechIds = new List<int>();
            }

            return model;
        }
    }

}