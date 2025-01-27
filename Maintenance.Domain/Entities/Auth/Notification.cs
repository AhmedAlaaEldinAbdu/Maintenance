﻿using Maintenance.Domain.Entities.Complanits;
using Maintenance.Domain.Enums;
using Maintenance.Domain.Interfaces;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Maintenance.Domain.Entities.Auth
{
    public class Notification : IBaseEntity, IAuditable, ISoftDelete
    {
        public long Id { get; set; }
        public State State { get ; set ; }
        public long? CreatedBy { get ; set ; }
        public DateTime CreatedOn { get ; set ; }
        public long? UpdatedBy { get ; set ; }
        public DateTime? UpdatedOn { get ; set ; }
        public long? From { get; set; }
        public long? To { get; set; }
        public bool? Read { get; set; }
        public string SubjectAr { get; set; }
        public string SubjectEn { get; set; }
        public string BodyAr { get; set; }
        public string BodyEn { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationType Type { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationState NotificationState { get; set; }
        public ICollection<UserNotification> Users { get; set; }  = new HashSet<UserNotification>();
        [ForeignKey("ComplanitHistoryId")]
        public long? ComplanitHistoryId { get; set; }
        public ComplanitHistory ComplanitHistory { get; set; }

        //[ForeignKey("RequestComplanitId")]
        //public long? RequestComplanitId { get; set; }
        //public RequestComplanit RequestComplanit { get; set; }
        public DateTime? ReadDate { get; set; }
    }
}
