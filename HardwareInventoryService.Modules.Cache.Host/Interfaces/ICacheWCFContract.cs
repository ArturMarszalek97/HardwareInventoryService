﻿using HardwareInventoryService.Models.Models;
using HardwareInventoryService.Models.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ExceptionDetail = HardwareInventoryService.Models.Models.ExceptionDetail;

namespace HardwareInventoryService.Modules.Cache.Host.Interfaces
{
    [ServiceContract]
    public interface ICacheWCFContract
    {
        #region Session

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void UpdateSession(Session session);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void AddSession(Session session);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        Session GetSessionByUsername(string username);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        Session[] GetSessionsByUsernamesArray(string[] username);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        IEnumerable<Session> GetSessions();

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void RemoveSessionByUsername(string username);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        IEnumerable<Session> GetLoggedSessions();

        #endregion

        #region User

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void AddUser(User user);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        User GetUserByUsername(string username);

        #endregion

        #region Item

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void AddItem(Item item);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void RemoveItem(Item item);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        void UpdateItem(Item item);

        [OperationContract]
        [FaultContract(typeof(ExceptionDetail))]
        List<Item> GetItems();

        #endregion
    }
}
