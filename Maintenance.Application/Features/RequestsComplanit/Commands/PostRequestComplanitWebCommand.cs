﻿using AuthDomain.Entities.Auth;
using AutoMapper;
using Maintenance.Application.GenericRepo;
using Maintenance.Application.Helper;
using Maintenance.Application.Helpers.CodeRandom;
using Maintenance.Application.Helpers.Notifications;
using Maintenance.Application.Helpers.QueryableExtensions;
using Maintenance.Application.Helpers.UploadHelper;
using Maintenance.Application.Interfaces;
using Maintenance.Domain.Entities.Auth;
using Maintenance.Domain.Entities.Complanits;
using Maintenance.Domain.Enums;
using Maintenance.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using Refit;
using System.Linq;
using System.Xml.Linq;

namespace Maintenance.Application.Features.RequestsComplanit.Commands
{
    public class PostRequestComplanitWebCommand : IRequest<ResponseDTO>
    {

        public string? Description { get; set; }
        public long[]? CheckListsRequest { get; set; }
        public string[]? CheckCategoriesRequest { get; set; }
        public IFormFileCollection? AttachmentsComplanit { get; set; }
        public string? SerialNumber { get; set; }
        public string? OfficeId { get; set; }
        public string? RegionId { get; set; }
        public string? ApplicantPhoneNumber { get; set; }
        public string? ApplicantName { get; set; }

        class PostRequestComplanit : IRequestHandler<PostRequestComplanitWebCommand, ResponseDTO>
        {
            private readonly IGRepository<RequestComplanit> _RequestComplanitRepository;

            private readonly IGRepository<ComplanitHistory> _ComplanitHistoryRepository;
            private readonly IGRepository<ComplanitFilter> _ComplanitFilterRepository;
            private readonly IGRepository<CheckListRequest> _CheckListRequestRepository;
            private readonly IGRepository<AttachmentComplanit> _AttachmentComplanitRepository;
            private long complaintId;

            private readonly ILogger<PostRequestComplanit> _logger;
            private readonly IStringLocalizer<string> _Localizer; 
            private readonly ResponseDTO _response;
            private readonly IMapper _mapper;
            private readonly IRoom _room;
            private readonly UserManager<User> _userManager;
            private readonly IGRepository<Notification> _NotificationRepository;
            public PostRequestComplanit(

                IGRepository<RequestComplanit> RequestComplanitRepository,
                ILogger<PostRequestComplanitCommand> logger,
                IGRepository<CheckListRequest> checkListRequestRepository,
                IGRepository<AttachmentComplanit> attachmentComplanitRepository,
                IMapper mapper,
                IGRepository<ComplanitHistory> ComplanitHistoryRepository,
                IStringLocalizer<string> Localizer,
                IRoom room,
                IGRepository<Notification> NotificationRepository,
                UserManager<User> userManager,
                IGRepository<ComplanitFilter> ComplanitFilterRepository,ILoggerFactory logFactory
            )
            {
                _logger = logFactory.CreateLogger<PostRequestComplanit>();

                _AttachmentComplanitRepository = attachmentComplanitRepository;
                _CheckListRequestRepository = checkListRequestRepository;
                _RequestComplanitRepository = RequestComplanitRepository;
                _response = new ResponseDTO();
                _mapper = mapper;
                _ComplanitHistoryRepository = ComplanitHistoryRepository;
                _Localizer = Localizer;
                _room = room;
                _NotificationRepository = NotificationRepository;
                _userManager = userManager;
                _ComplanitFilterRepository = ComplanitFilterRepository;
            }
            public async Task<ResponseDTO> Handle(PostRequestComplanitWebCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    RoomsDTO room = new RoomsDTO();
                    try
                    {
                        room = await _room.GetRoomId(request.SerialNumber);

                        if (room == null)

                        {
                            _response.Result = request.SerialNumber;

                            _response.StatusEnum = StatusEnum.Failed;

                            _response.Message = _Localizer["RoomNotFound"].ToString();

                            return _response;
                        }

                    }
                    catch (ApiException ex)
                    {
                        _response.Result = null;

                        _response.StatusEnum = StatusEnum.Failed;

                        _logger.LogError(ex, ex.Message, ex != null && ex.InnerException != null ? ex.InnerException.Message : "");
                        _response.Message = _Localizer["anErrorOccurredPleaseContactSystemAdministrator"];
                        return _response;
                    }



                    var foundedUsers = _userManager.Users.Where(x => x.IdentityNumber == request.ApplicantPhoneNumber
                    || x.PhoneNumber == request.ApplicantPhoneNumber).FirstOrDefault();
                    if (foundedUsers != null)
                    {
                        foundedUsers.IdentityNumber = request.ApplicantPhoneNumber;
                        foundedUsers.PhoneNumber = request.ApplicantPhoneNumber;
                        foundedUsers.FullName = request.ApplicantName;
                        await _userManager.UpdateAsync(foundedUsers);
                    }
                    else
                    {
                        foundedUsers = new User()
                        {
                            CreatedOn = DateTime.Now,
                            CreatedBy = null,
                            Email = request.ApplicantPhoneNumber + "@Gamil.com",
                            FullName = request.ApplicantName,
                            IdentityNumber = request.ApplicantPhoneNumber,
                            OfficeId = Convert.ToInt32(request.OfficeId),
                            RegionId = Convert.ToInt32(request.RegionId),
                            UserType = UserType.Client,
                            PhoneNumber = request.ApplicantPhoneNumber,
                            UserName = request.ApplicantPhoneNumber,
                            State = State.NotDeleted,
                            NormalizedUserName = request.ApplicantPhoneNumber,
                            NormalizedEmail = request.ApplicantPhoneNumber + "@Gamil.com",
                        };

                        var result = await _userManager.CreateAsync(foundedUsers,"P@ssw0rd");
                        if (!result.Succeeded)
                        {
                            _response.Result = null;
                            _response.StatusEnum = StatusEnum.Exception;
                            _logger.LogError(result.Errors.FirstOrDefault().Description);
                            _response.Message = result.Errors.FirstOrDefault().Description;
                            //   _responseDTO.Message = _localizationProvider.Localize("anErrorOccurredPleaseContactSystemAdministrator", _auditService.UserLanguage);
                            return _response;
                        }

                    }


                    var lastComplanit = await _RequestComplanitRepository.GetAll().OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                    var lastCode = GenerateRandomNumber.GetSerial(Convert.ToInt64(lastComplanit != null ? lastComplanit.Code : "0"));

                    var RequestComplanit = new RequestComplanit()
                    {
                        CreatedBy = foundedUsers.Id,
                        CreatedOn = DateTime.Now,
                        State = Domain.Enums.State.NotDeleted,
                        Description = request.Description,
                        SerialNumber = request.SerialNumber,
                        Code = lastCode,
                        ComplanitStatus = ComplanitStatus.Submitted,
                        OfficeId = room.OfficeId,
                        RegionId = room.RegionId
                    };
                    complaintId = RequestComplanit.Id;
                    foreach (var item in request.AttachmentsComplanit)
                    {
                        var itemPath = Upload.SaveFile(item, RequestComplanit.Id);
                        RequestComplanit.AttachmentsComplanit.Add(new AttachmentComplanit()
                        {
                            Path = RequestComplanit.Id + "/" + itemPath,
                            CreatedBy = foundedUsers.Id,
                            CreatedOn = DateTime.Now,
                            RequestComplanitId = RequestComplanit.Id,
                            State = Domain.Enums.State.NotDeleted,
                            Name = item.Name
                        });
                    }

                    foreach (var item in request.CheckListsRequest)
                    {
                        RequestComplanit.CheckListRequests.Add(new CheckListRequest()
                        {
                            CheckListComplanitId = item,
                            CreatedBy = foundedUsers.Id,
                            CreatedOn = DateTime.Now,
                        });
                    }


                    var ComplanitHistory = new ComplanitHistory()
                    {
                        CreatedBy = foundedUsers.Id,
                        CreatedOn = DateTime.Now,
                        State = Domain.Enums.State.NotDeleted,
                        ComplanitStatus = Domain.Enums.ComplanitStatus.Submitted,

                    };

                    //////////
                    ///

                    //long? RegionId = null;
                    //long? OfficeId = null;

                    //if (room.RegionId != null && room.RegionId > 0)
                    //{
                    //    var RegionSearch = ComplanitFilterList.Any(c => c.RegionId.Split(',', StringSplitOptions.None).
                    //    Contains(room.RegionId.ToString()));
                    //    if (RegionSearch)
                    //    {
                    //        RegionId = room.RegionId;
                    //    }

                    //}
                    //if (room.OfficeId != null && room.OfficeId > 0)
                    //{
                    //    var OfficeSearch = ComplanitFilterList.Any(c => c.OfficeId.Split(',', StringSplitOptions.None).
                    //    Contains(room.OfficeId.ToString()));
                    //    if (OfficeSearch)
                    //    {
                    //        OfficeId = room.OfficeId;
                    //    }
                    //}
                    //if (request.CategoryComplanitId != null && request.CategoryComplanitId > 0)
                    //{
                    //    ComplanitFilterListTemp = ComplanitFilterListTemp.Where(c => c.CategoryComplanitId.Split(',', StringSplitOptions.None).Contains(request.CategoryComplanitId.ToString())).ToList();

                    //}

                    //ComplanitFilterListTemp = ComplanitFilterListTemp.Where(c => c.OfficeId.Split(',', StringSplitOptions.None).Contains(OfficeId.ToString())).ToList();


                    //ComplanitFilterListTemp = ComplanitFilterListTemp.Where(c => c.RegionId.Split(',', StringSplitOptions.None).Contains(RegionId.ToString())).ToList();

                    var ComplanitFilterList =  _ComplanitFilterRepository.GetAll(x => x.State == State.NotDeleted).ToList();

                    var ComplanitFilterListTemp = ComplanitFilterList;


                    long? RegionId = null;
                    long? OfficeId = null;

                    if (room.RegionId != null && room.RegionId > 0)
                    {
                        var RegionSearch = ComplanitFilterList.Any(c => c.RegionId.Split(',', StringSplitOptions.None).
                        Contains(room.RegionId.ToString()));
                        if (RegionSearch)
                        {
                            RegionId = room.RegionId;
                        }

                    }
                    if (room.OfficeId != null && room.OfficeId > 0)
                    {
                        var OfficeSearch = ComplanitFilterList.Any(c => c.OfficeId.Split(',', StringSplitOptions.None).
                        Contains(room.OfficeId.ToString()));
                        if (OfficeSearch)
                        {
                            OfficeId = room.OfficeId;
                        }
                    }

                    ComplanitFilterList = ComplanitFilterList.
                        Where(
                        c => c.OfficeId.Split(',', StringSplitOptions.None).Contains(OfficeId.ToString())
                        &&
                        c.RegionId.Split(',', StringSplitOptions.None).Contains(RegionId.ToString())
                        &&
                         request.CheckCategoriesRequest.Any(a=> c.CategoryComplanitId.Contains(a))
                        ).ToList();
                    //if (ComplanitFilterList.Count == 0)
                    //{
                    //    ComplanitFilterList = ComplanitFilterListTemp;
                    //}

                    //if (room.RegionId != null && room.RegionId > 0)
                    //{
                    //    ComplanitFilterList = ComplanitFilterList.Where(c => c.RegionId.Split(',', StringSplitOptions.None).Contains(room.RegionId.ToString())).ToList();
                    //    if (ComplanitFilterList.Count == 0)
                    //    {
                    //        ComplanitFilterList = ComplanitFilterListTemp;
                    //    }
                    //}
                    //if (request.CategoryComplanitId != null && request.CategoryComplanitId > 0)
                    //{
                    //    ComplanitFilterList = ComplanitFilterList.Where(c => c.CategoryComplanitId.Split(',', StringSplitOptions.None).Contains(request.CategoryComplanitId.ToString())).ToList();

                    //}

                    var usersIds = ComplanitFilterList.Select(c => c.CreatedBy).ToList();

                    var users = _userManager.Users.Where(x => x.State == State.NotDeleted)

                        .WhereIf(usersIds.Count > 0, x => (x.UserType == UserType.Owner || x.UserType == UserType.Consultant || (x.UserType == UserType.Technician && ComplanitFilterList.Select(f => f.CreatedBy).Contains(x.Id))))
                        .WhereIf(usersIds.Count == 0, x => (x.UserType == UserType.Owner || x.UserType == UserType.Consultant))

                        .ToList();
                    foreach (var item in users)

                    {
                        var notfication = new Notification()
                        {
                            CreatedBy = foundedUsers.Id,

                            CreatedOn = DateTime.Now,

                            State = Domain.Enums.State.NotDeleted,

                            From = foundedUsers.Id,

                            NotificationState = NotificationState.New,

                            SubjectAr = RequestComplanit.Code,

                            SubjectEn = RequestComplanit.Code,

                            BodyAr = _Localizer["ResponsesToComplaint"],

                            BodyEn = _Localizer["ResponsesToComplaint", "en"],

                            To = item.Id,

                            Read = false,

                            Type = NotificationType.Message
                        };



                        var notificationDto = new NotificationDto()
                        {
                            Title = RequestComplanit.Code,
                            Body = _Localizer["ResponsesToComplaint"]
                        };

                        await NotificationHelper.FCMNotify(notificationDto, item.Token);
                        ComplanitHistory.Notifications.Add(notfication);
                    }

                    RequestComplanit.ComplanitHistory.Add(ComplanitHistory);

                    _RequestComplanitRepository.Add(RequestComplanit);

                    _RequestComplanitRepository.Save();

                    _response.StatusEnum = StatusEnum.SavedSuccessfully;
                    _response.Message = "AddedSuccessfully";
                    return _response;
                }
                catch (Exception ex)
                {

                    var folderName = Path.Combine("wwwroot/Uploads/Complanits");

                       try
                        {
                            Directory.Delete(folderName+"/"+ complaintId.ToString());
                        }
                        catch { }
                    _response.StatusEnum = StatusEnum.Exception;
                    _response.Result = null;
                    _response.Message = ex != null && ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    _logger.LogError(ex, ex.Message, ex != null && ex.InnerException != null ? ex.InnerException.Message : "");

                    return _response;
                }
            }
            public string GenerateCodeComplaint()
            {
                var characters = "0123456789";
                var charsArr = new char[10];
                var random = new Random();
                for (int i = 0; i < charsArr.Length; i++)
                {
                    charsArr[i] = characters[random.Next(characters.Length)];
                }
                var segmentString = new String(charsArr);
                return segmentString;
            }
        }
    }
}
