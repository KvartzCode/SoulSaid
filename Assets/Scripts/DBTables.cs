using System;
using UnityEngine;

namespace DBTables
{
    [Serializable]
    public class MessageLocation
    {
        public string Text;
        public float Latitude;
        public float Longitude;
        public string UserID;
        public DateTime DateCreated = DateTime.Now;


        public MessageLocation() { }

        public MessageLocation(LocationInfo locationInfo, string text, string user, DateTime? date = null)
        {
            Text = text;
            Longitude = locationInfo.longitude;
            Latitude = locationInfo.latitude;
            UserID = user;
            DateCreated = date != null ? (DateTime)date : DateCreated;
        }
    }
}
