using FarmerBrothers.Data;
using System;
using System.Web.Mvc;

namespace FarmerBrothers.Models
{
    public class NotesHistoryModel
    {
        public NotesHistoryModel(NotesHistory notesHistory)
        {
            ErfID = notesHistory.ErfID;
            WorkorderID = notesHistory.WorkorderID;
            FeastMovementID = notesHistory.FeastMovementID;
            Userid = notesHistory.Userid;
            UserName = notesHistory.UserName;
            AutomaticNotes = notesHistory.AutomaticNotes;
            NotesID = notesHistory.NotesID;
            EntryDate = notesHistory.EntryDate;
            Notes = "[" + UserName + "] - " + (EntryDate.HasValue ? EntryDate.Value.ToString("MM/dd/yyyy hh:mm:ss tt") : " ") + " - " + notesHistory.Notes;
        }

        public string ErfID { get; set; }
        public Nullable<int> WorkorderID { get; set; }
        public Nullable<int> FeastMovementID { get; set; }
        public Nullable<int> Userid { get; set; }
        public string UserName { get; set; }
        [AllowHtml]
        public string Notes { get; set; }
        public Nullable<short> AutomaticNotes { get; set; }
        public int NotesID { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
    }
}