using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        
        [JsonIgnore]
        public DateTime DateCreated = DateTime.Now;

        [JsonProperty("DateCreated")]
        private int DateCreatedTicks
        {
            get { return (int)(this.DateCreated - DateTime.UnixEpoch).TotalSeconds; }
            set { this.DateCreated = DateTime.UnixEpoch.AddSeconds(Convert.ToInt32(value)); }
        }


        public MessageLocation() { }

        public MessageLocation(LocationInfo locationInfo, string text, string user, DateTime? date = null)
        {
            Text = text;
            Latitude = locationInfo.latitude;
            Longitude = locationInfo.longitude;
            UserID = user;
            DateCreated = date != null ? (DateTime)date : DateCreated;
        }
    }
}
