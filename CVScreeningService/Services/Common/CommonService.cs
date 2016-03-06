using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Common;
using CVScreeningService.Filters;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.Common
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class CommonService : ICommonService
    {
        private readonly IUnitOfWork _uow;

        public CommonService(IUnitOfWork uow)
        {
            _uow = uow;
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<Location, LocationDTO>();
            Mapper.CreateMap<Post, PostDTO>();
            Mapper.CreateMap<ContactInfo, ContactInfoDTO>();
            Mapper.CreateMap<ContactPerson, ContactPersonDTO>();
        }

        #region Address module

        //
        // Address module
        //

        /// <summary>
        /// Get all the addresses
        /// </summary>
        /// <returns></returns>
        public virtual List<AddressDTO> GetAllAddresses()
        {
            var addresses = _uow.AddressRepository.GetAll();
            return addresses.Select(Mapper.Map<Address, AddressDTO>).ToList();
        }

        /// <summary>
        /// Service to create address
        /// </summary>
        /// <param name="addressDto"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateAddress(
            ref AddressDTO addressDto, bool isCommited = true)
        {
            try
            {
                var locationId = addressDto != null && addressDto.Location != null ? addressDto.Location.LocationId : 0;
                var newAddress = new Address
                {
                    Street = addressDto.Street,
                    PostalCode = addressDto.PostalCode,
                    Location = locationId > 0 ? _uow.LocationRepository.Single(l => l.LocationId == locationId) : null
                };

                newAddress = _uow.AddressRepository.Add(newAddress);
                if (isCommited)
                    _uow.Commit();

                addressDto = Mapper.Map<Address, AddressDTO>(newAddress);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        addressDto.Location.LocationId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Service to delete an address
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteAddress(int addressId, bool isCommited = true)
        {
            try
            {
                if (!_uow.AddressRepository.Exist(u => u.AddressId == addressId))
                {
                    return ErrorCode.COMMON_ADDRESS_NOT_FOUND;
                }

                var address = _uow.AddressRepository.Single(u => u.AddressId == addressId);
                _uow.AddressRepository.Delete(address);

                if (isCommited)
                    _uow.Commit();
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        addressId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        #endregion

        #region Location module

        //
        // Location module
        //
        public virtual List<LocationDTO> GetAllLocations()
        {
            var locations = _uow.LocationRepository.GetAll();
            return locations.Select(Mapper.Map<Location, LocationDTO>).ToList();
        }

        public virtual List<LocationDTO> GetAllLocationsByLevel(int locationLevel)
        {
            var locations = _uow.LocationRepository.GetAll().ToList().Where(l => l.LocationLevel == locationLevel);
            return locations.Select(Mapper.Map<Location, LocationDTO>).ToList();
        }

        /// <summary>
        /// Service to get location by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual LocationDTO GetLocation(int? id)
        {
            try
            {
                if (!_uow.LocationRepository.Exist(u => u.LocationId == id))
                {
                    return null;
                }

                var location = _uow.LocationRepository.Single(l => l.LocationId == id);
                return Mapper.Map<Location, LocationDTO>(location);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        id, ex.Message));
                return null;
            }
        }

        public virtual LocationDTO GetLocationByName(string locationName)
        {
            try
            {
                if (!_uow.LocationRepository.Exist(
                    l => string.Compare(l.LocationName, locationName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return null;
                }

                var location = _uow.LocationRepository.Single(
                    l => string.Compare(l.LocationName, locationName, StringComparison.OrdinalIgnoreCase) == 0);

                return Mapper.Map<Location, LocationDTO>(location);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Name: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        locationName, ex.Message));
                return null;
            }
        }

        public virtual LocationDTO GetLocationParent(int id, LocationDTO.LocationLevelEnum level)
        {
            try
            {
                if (!_uow.LocationRepository.Exist(u => u.LocationId == id))
                {
                    return null;
                }

                var subDistrict = _uow.LocationRepository.First(l => l.LocationId == id);
                Mapper.CreateMap<Location, LocationDTO>();

                if (level == LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT)
                    return Mapper.Map<Location, LocationDTO>(subDistrict.LocationParent);
                if (level == LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY)
                    return Mapper.Map<Location, LocationDTO>(subDistrict.LocationParent.LocationParent);
                if (level == LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE)
                    return Mapper.Map<Location, LocationDTO>(subDistrict.LocationParent.LocationParent.LocationParent);
                if (level == LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY)
                    return Mapper.Map<Location, LocationDTO>(subDistrict.LocationParent.LocationParent.
                        LocationParent.LocationParent);

                return Mapper.Map<Location, LocationDTO>(subDistrict);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(string.Format(
                    "GetLocationParent failed for id={1}, level={2}. Error={3}", id, level, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Service to create a new location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateLocation(ref LocationDTO location, bool commit = true)
        {
            try
            {
                var locationName = location.LocationName;
                var locationLevel = location.LocationLevel;
                var parentLocationId = location.LocationParentLocationId;

                if (_uow.LocationRepository.Exist(u => u.LocationName == locationName
                                                       && u.LocationLevel == locationLevel))
                {
                    return ErrorCode.COMMON_LOCATION_ALREADY_EXISTS;
                }

                var newLoc = new Location
                {
                    LocationName = location.LocationName,
                    LocationLevel = location.LocationLevel,
                    LocationParent = parentLocationId != null
                        ? _uow.LocationRepository.Single(l => l.LocationId == parentLocationId)
                        : null
                };

                newLoc = _uow.LocationRepository.Add(newLoc);

                if (commit)
                {
                    _uow.Commit();
                    location.LocationId = newLoc.LocationId;
                }
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        location.LocationId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        public virtual bool HaveSameDirectParent(params LocationDTO[] locations)
        {
            for (int iterator1 = 0,
                starter = 0,
                length = locations.Length;
                iterator1 < length;
                iterator1++)
            {
                iterator1 = starter;
                for (int iterator2 = starter + 1;
                    iterator2 < length;
                    iterator2++)
                {
                    if (locations[iterator1].LocationParent == null ||
                        locations[iterator2].LocationParent == null)
                        continue;

                    if (locations[iterator1].LocationParent.LocationId !=
                        locations[iterator2].LocationParent.LocationId)
                        return false;
                }
                starter ++;
            }

            return true;
        }

        #endregion

        #region Contact Info module

        //
        // Contact info module
        //

        public virtual ErrorCode CreateContactInfo(ref ContactInfoDTO contactInfoDTO, bool isCommited = true)
        {
            try
            {
                var newContactInfo = new ContactInfo
                {
                    HomePhoneNumber = contactInfoDTO.HomePhoneNumber,
                    WorkPhoneNumber = contactInfoDTO.WorkPhoneNumber,
                    MobilePhoneNumber = contactInfoDTO.MobilePhoneNumber,
                    OfficialEmail = contactInfoDTO.OfficialEmail,
                    WebSite = contactInfoDTO.WebSite,
                    Position = contactInfoDTO.Position
                };

                newContactInfo = _uow.ContactInfoRepository.Add(newContactInfo);

                if (isCommited)
                    _uow.Commit();

                contactInfoDTO = Mapper.Map<ContactInfo, ContactInfoDTO>(newContactInfo);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        contactInfoDTO.ContactInfoId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Service to delete an address
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteContactInfo(int contactInfoId, bool isCommited = true)
        {
            try
            {
                if (!_uow.ContactInfoRepository.Exist(u => u.ContactInfoId == contactInfoId))
                {
                    return ErrorCode.COMMON_CONTACT_INFO_NOT_FOUND;
                }

                var contactInfo = _uow.ContactInfoRepository.Single(u => u.ContactInfoId == contactInfoId);
                _uow.ContactInfoRepository.Delete(contactInfo);

                if (isCommited)
                    _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        contactInfoId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        #endregion

        #region Contact Person module

        //
        // Contact person module
        //

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual List<ContactPersonDTO> GetAllContactPersons()
        {
            var contactPersons = _uow.ContactPersonRepository.GetAll();
            return contactPersons.Select(Mapper.Map<ContactPerson, ContactPersonDTO>).ToList();
        }

        /// <summary>
        /// Service to get contact person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ContactPersonDTO GetContactPerson(int id)
        {
            try
            {
                if (!_uow.ContactPersonRepository.Exist(u => u.ContactPersonId == id))
                {
                    return null;
                }

                var contactPerson = _uow.ContactPersonRepository.First(l => l.ContactPersonId == id);
                return Mapper.Map<ContactPerson, ContactPersonDTO>(contactPerson);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        id, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Service to create contact person
        /// </summary>
        /// <param name="contactPersonDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateContactPerson(ref ContactPersonDTO contactPersonDTO, bool commit = true)
        {
            Address address = null;
            ContactInfo contactInfo = null;

            if (contactPersonDTO.Address != null)
            {
                var locationId = contactPersonDTO.Address.Location != null &&
                                 contactPersonDTO.Address.Location.LocationId != 0
                    ? contactPersonDTO.Address.Location.LocationId
                    : 0;
                address = new Address
                {
                    Street = contactPersonDTO.Address.Street,
                    PostalCode = contactPersonDTO.Address.PostalCode,
                    Location = locationId != 0 ? _uow.LocationRepository.First(l => l.LocationId == locationId) : null,
                };
                address = _uow.AddressRepository.Add(address);
            }

            if (contactPersonDTO.ContactInfo != null)
            {
                contactInfo = new ContactInfo
                {
                    OfficialEmail = contactPersonDTO.ContactInfo.OfficialEmail,
                    Position = contactPersonDTO.ContactInfo.Position,
                    WebSite = contactPersonDTO.ContactInfo.WebSite,
                    WorkPhoneNumber = contactPersonDTO.ContactInfo.WorkPhoneNumber,
                    HomePhoneNumber = contactPersonDTO.ContactInfo.HomePhoneNumber,
                    MobilePhoneNumber = contactPersonDTO.ContactInfo.MobilePhoneNumber
                };
                contactInfo = _uow.ContactInfoRepository.Add(contactInfo);
            }

            var contactPersonId = contactPersonDTO.ContactPersonId;
            var isExist = _uow.ContactPersonRepository
                .Exist(p => p.ContactPersonId == contactPersonId);

            var contactPerson = isExist
                ? _uow.ContactPersonRepository.First(
                    c => c.ContactPersonId == contactPersonId)
                : new ContactPerson();

            contactPerson.ContactPersonName = contactPersonDTO.ContactPersonName;
            contactPerson.Address = address;
            contactPerson.ContactInfo = contactInfo;
            contactPerson.ContactComments = contactPersonDTO.ContactComments;

            if (contactPersonDTO.QualificationPlaceId != 0)
            {
                //if not valid based on name and if it is not in edit mode
                if (!ValidateExistingContactPersonNameOnQualificationPlace(contactPersonDTO) && !isExist)
                    return ErrorCode.COMMON_CONTACT_PERSON_ALREADY_EXIST;

                var qualificationPlaceId = contactPersonDTO.QualificationPlaceId;
                contactPerson.QualificationPlace.Add(
                    _uow.QualificationPlaceRepository.First(
                        q => q.QualificationPlaceId == qualificationPlaceId));
            }

            if (contactPersonDTO.ClientCompanyId != 0)
            {
                //if not valid based on name and if it is not in edit mode
                if (!ValidateExistingContactPersonNameOnClientCompany(contactPersonDTO) && !isExist)
                    return ErrorCode.COMMON_CONTACT_PERSON_ALREADY_EXIST;

                var clientCompanyId = contactPersonDTO.ClientCompanyId;
                contactPerson.ClientCompany = _uow.ClientCompanyRepository
                    .First(q => q.ClientCompanyId == clientCompanyId);
            }

            try
            {
                _uow.ContactPersonRepository.Add(contactPerson);
                if (commit)
                    _uow.Commit();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        contactPersonDTO.ContactPersonId, ex.Message));
                return ErrorCode.COMMON_CONTACT_PERSON_INSERT_ERROR;
            }

            contactPersonDTO.ContactPersonId = contactPerson.ContactPersonId;
            return ErrorCode.NO_ERROR;
        }

        private bool ValidateExistingContactPersonNameOnQualificationPlace(ContactPersonDTO contactPersonDTO)
        {
            var allContactPersons = _uow.ContactPersonRepository.GetAll();
            foreach (var contactPerson in allContactPersons)
            {
                if (!contactPerson.QualificationPlace.Select(e => e.QualificationPlaceId)
                    .Contains(contactPersonDTO.QualificationPlaceId))
                    continue;
                if (contactPerson.ContactPersonName.Equals(contactPersonDTO.ContactPersonName))
                    return false;
            }
            return true;
        }

        private bool ValidateExistingContactPersonNameOnClientCompany(ContactPersonDTO contactPersonDTO)
        {
            foreach (var contactPerson in _uow.ContactPersonRepository.GetAll())
            {
                if (contactPerson.ContactPersonId != contactPersonDTO.ContactPersonId)
                    continue;
                if (contactPerson.ContactPersonName.ToLower().Equals(contactPersonDTO.ContactPersonName.ToLower()))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Delete contact of a person
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteContactPerson(int id, bool commit = true)
        {
            try
            {
                if (!_uow.ContactPersonRepository.Exist(u => u.ContactPersonId == id))
                {
                    return ErrorCode.COMMON_CONTACT_PERSON_NOT_FOUND;
                }

                var contactPerson = _uow.ContactPersonRepository.First(c => c.ContactPersonId == id);
                if (contactPerson.QualificationPlace != null)
                {
                    foreach (var qualificationPlace in contactPerson.QualificationPlace.Reverse())
                    {
                        qualificationPlace.ContactPerson.Remove(contactPerson);
                    }
                }

                _uow.ContactPersonRepository.Delete(contactPerson);

                if (commit)
                    _uow.Commit();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        id, ex.Message));
                return ErrorCode.COMMON_CONTACT_PERSON_DELETE_ERROR;
            }
            return ErrorCode.NO_ERROR;
        }

        #endregion

        #region Post module

        //
        // Post module
        //

        public virtual PostDTO GetPostByName(string postTitle)
        {
            try
            {
                var post = _uow.PostRepository.Single(
                    p => string.Compare(p.PostTitle, postTitle, StringComparison.OrdinalIgnoreCase) == 0);

                return Mapper.Map<Post, PostDTO>(post);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Name: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        postTitle, ex.Message));
                return new PostDTO {PostContent = "", PostTitle = ""};
            }
        }

        public virtual Post InsertPost(PostDTO postData)
        {
            var newPost = new Post
            {
                PostContent = postData.PostContent,
                PostTitle = postData.PostTitle
            };

            //find if it is existing based on post title
            var existPost = _uow.PostRepository.First(
                p => string.Compare(p.PostTitle, postData.PostTitle, StringComparison.OrdinalIgnoreCase) == 0);
            if (existPost != null)
            {
                existPost = newPost;
                return existPost;
            }
            newPost = _uow.PostRepository.Add(newPost);
            return newPost;
        }

        public ErrorCode DeactivateContactPerson(int id)
        {
            var contactPerson = _uow.ContactPersonRepository.First(e => e.ContactPersonId == id);

            contactPerson.IsContactDeactivated = true;
            _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        public ErrorCode ActivateContactPerson(int id)
        {
            var contactPerson = _uow.ContactPersonRepository.First(e => e.ContactPersonId == id);

            contactPerson.IsContactDeactivated = false;
            _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        #endregion
    }
}