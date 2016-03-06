using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Common;

namespace CVScreeningService.Services.Common
{
    public interface ICommonService
    {
        #region Address module

        /// <summary>
        ///     Get all addresses stored in Database without their location
        /// </summary>
        /// <returns></returns>
        List<AddressDTO> GetAllAddresses();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addressDto"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        ErrorCode CreateAddress(ref AddressDTO addressDto, bool isCommited = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addressDto"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        ErrorCode DeleteAddress(int addressId, bool isCommited = true);

        #endregion

        #region Location module

        /// <summary>
        ///     Get all locations stored in Database without their hierarchy
        /// </summary>
        /// <returns></returns>
        List<LocationDTO> GetAllLocations();

        /// <summary>
        ///     Get all locations stored in Database based on given location level
        /// </summary>
        /// <param name="locationLevel">1: Nation, 2: Province, 3: District, 4: Subdistrict, 5: Village/Urban</param>
        /// <returns></returns>
        List<LocationDTO> GetAllLocationsByLevel(int locationLevel);


        /// <summary>
        ///     Get the specific location based on given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        LocationDTO GetLocation(int? id);

        /// <summary>
        ///     Get DTO of location based on given location name
        /// </summary>
        /// <param name="locationName"></param>
        /// <returns></returns>
        LocationDTO GetLocationByName(string locationName);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id">subDistrict Id</param>
        /// <param name="level">level that needs to retrieved</param>
        /// <returns></returns>
        LocationDTO GetLocationParent(int id, LocationDTO.LocationLevelEnum level);

        /// <summary>
        /// Service to create a location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateLocation(ref LocationDTO location, bool commit = true);

        /// <summary>
        /// Check if the given locations have same parent
        /// </summary>
        /// <param name="locations">Given location to compare</param>
        /// <returns></returns>
        bool HaveSameDirectParent(params LocationDTO[] locations);

        #endregion

        #region Contact Info module

        /// <summary>
        /// Service to create a contact info
        /// </summary>
        /// <param name="contactInfoDTO"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        ErrorCode CreateContactInfo(ref ContactInfoDTO contactInfoDTO, bool isCommited = true);

        /// <summary>
        /// Service to delete a contact info
        /// </summary>
        /// <param name="contactInfoId"></param>
        /// <param name="isCommited"></param>
        /// <returns></returns>
        ErrorCode DeleteContactInfo(int contactInfoId, bool isCommited = true);

        #endregion

        #region Contact Person module

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        List<ContactPersonDTO> GetAllContactPersons();

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ContactPersonDTO GetContactPerson(int id);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="contactPersonDTO"></param>
        /// <returns></returns>
        ErrorCode CreateContactPerson(ref ContactPersonDTO contactPersonDTO, bool commit = true);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ErrorCode DeleteContactPerson(int id, bool commit = true);

        #endregion

        #region Post module

        /// <summary>
        ///     Get DTO of post based on given post title
        /// </summary>
        /// <param name="postTitle"></param>
        /// <returns></returns>
        PostDTO GetPostByName(string postTitle);

        /// <summary>
        ///     Insert or update post
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        Post InsertPost(PostDTO postData);

        #endregion

        /// <summary>
        ///     Set isDeactivated to true
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ErrorCode DeactivateContactPerson(int id);

        /// <summary>
        ///     Set isDeactivated to false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ErrorCode ActivateContactPerson(int id);
    }
}