using System.Linq;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Services.Settings;
using NUnit.Framework;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.ErrorHandling;
using System;

namespace CVScreeningService.Tests.UnitTest.UserManagement
{
    [TestFixture]
    public class PublicHoliday
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ISettingsService _settingsService;
        private IErrorMessageFactoryService _errorMessageFactoryService;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _settingsService = new SettingsService(_unitOfWork);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var publicHolidays = _unitOfWork.PublicHolidayRepository.GetAll();
            foreach (var publicHoliday in publicHolidays.Reverse())
            {
                _unitOfWork.PublicHolidayRepository.Delete(publicHoliday);
            }
        }

        /// <summary>
        // Test to create a public holiday
        /// </summary>
        [Test]
        public void CreatePublicHoliday()
        {
            // Public holiday from today to today + 1
            var publicHolidayDTO = new PublicHolidayDTO
            {
                PublicHolidayName = "Idul Fitri",
                PublicHolidayRemarks = "Selamat Idul Fitri",
                PublicHolidayStartDate = DateTime.Now,
                PublicHolidayEndDate = DateTime.Now.AddDays(1)
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

        }

        /// <summary>
        // Test to create a public holiday that is overlapping with a previous one
        /// </summary>
        [Test]
        public void CreatePublicHolidayOverlapping()
        {
            // Public holiday from today to today + 2
            var publicHolidayDTO1 = new PublicHolidayDTO
            {
                PublicHolidayName = "Idul Fitri",
                PublicHolidayRemarks = "Selamat Idul Fitri",
                PublicHolidayStartDate = DateTime.Now,
                PublicHolidayEndDate = DateTime.Now.AddDays(2)
            };
            var error = _settingsService.CreatePublicHoliday(ref publicHolidayDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            // Public holiday from today to today + 2
            var publicHolidayDTO2 = new PublicHolidayDTO
            {
                PublicHolidayName = "Idul Fitri",
                PublicHolidayRemarks = "Selamat Idul Fitri",
                PublicHolidayStartDate = DateTime.Now,
                PublicHolidayEndDate = DateTime.Now.AddDays(2)
            };
            error = _settingsService.CreatePublicHoliday(ref publicHolidayDTO2);

            // Test that service is returning overlap error
            Assert.AreEqual(error, ErrorCode.PUBLIC_HOLIDAY_DATE_OVERLAPPING);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());
        }

        /// <summary>
        /// Test to edit a public holiday
        /// </summary>
        [Test]
        public void EditPublicHoliday()
        {
            // Public holiday from today + 2 to today + 3
            var publicHolidayCreate = new PublicHolidayDTO
            {
                PublicHolidayName = "Idul Fitri",
                PublicHolidayRemarks = "Selamat Idul Fitri",
                PublicHolidayStartDate = DateTime.Now.AddDays(2),
                PublicHolidayEndDate = DateTime.Now.AddDays(3)
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayCreate);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            // Public holiday from today + 1 to today + 2
            var publicHolidayEdit = new PublicHolidayDTO
            {
                PublicHolidayId = publicHolidayCreate.PublicHolidayId,
                PublicHolidayName = "Idul Fitri Edit",
                PublicHolidayRemarks = "Selamat Idul Fitri Edit",
                PublicHolidayStartDate = DateTime.Now.AddDays(1),
                PublicHolidayEndDate = DateTime.Now.AddDays(2)
            };

            error = _settingsService.EditPublicHoliday(publicHolidayEdit);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            var publicHolidayGet = _settingsService.GetPublicHoliday(publicHolidayCreate.PublicHolidayId);

            Assert.AreEqual(publicHolidayEdit.PublicHolidayId, publicHolidayGet.PublicHolidayId);
            Assert.AreEqual(publicHolidayEdit.PublicHolidayName, publicHolidayGet.PublicHolidayName);
            Assert.AreEqual(publicHolidayEdit.PublicHolidayStartDate.Date, publicHolidayGet.PublicHolidayStartDate);
            Assert.AreEqual(publicHolidayEdit.PublicHolidayEndDate.Date, publicHolidayGet.PublicHolidayEndDate);
            Assert.AreEqual(publicHolidayEdit.PublicHolidayRemarks, publicHolidayGet.PublicHolidayRemarks);


        }

        /// <summary>
        // Test to edit a public holiday that is overlapping with a previous one
        /// </summary>
        [Test]
        public void EditPublicHolidayOverlapping()
        {
            // Public holiday from today to today + 2
            var publicHolidayCreate1 = new PublicHolidayDTO
            {
                PublicHolidayName = "My Holiday 1",
                PublicHolidayRemarks = "My remarks 1",
                PublicHolidayStartDate = DateTime.Now.AddDays(0),
                PublicHolidayEndDate = DateTime.Now.AddDays(2)
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayCreate1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            // Public holiday from today + 3 to today + 4
            var publicHolidayCreate2 = new PublicHolidayDTO
            {
                PublicHolidayName = "My Holiday 1",
                PublicHolidayRemarks = "My remarks 1",
                PublicHolidayStartDate = DateTime.Now.AddDays(3),
                PublicHolidayEndDate = DateTime.Now.AddDays(4)
            };

            error = _settingsService.CreatePublicHoliday(ref publicHolidayCreate2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, _unitOfWork.PublicHolidayRepository.CountAll());

            // Public holiday from today + 1 to today + 2, overlapping with publicHolidayCreate1
            var publicHolidayEdit = new PublicHolidayDTO
            {
                PublicHolidayId = publicHolidayCreate2.PublicHolidayId,
                PublicHolidayName = "My Holiday 3",
                PublicHolidayRemarks = "My remarks 3",
                PublicHolidayStartDate = DateTime.Now.AddDays(1),
                PublicHolidayEndDate = DateTime.Now.AddDays(2)
            };

            error = _settingsService.EditPublicHoliday(publicHolidayEdit);
            Assert.AreEqual(ErrorCode.PUBLIC_HOLIDAY_DATE_OVERLAPPING, error);
            Assert.AreEqual(2, _unitOfWork.PublicHolidayRepository.CountAll());
        }

        /// <summary>
        // Test to delete a public holiday
        /// </summary>
        [Test]
        public void DeletePublicHoliday()
        {
            // Public holiday from today to today + 1
            var publicHolidayDTO = new PublicHolidayDTO
            {
                PublicHolidayName = "Idul Fitri",
                PublicHolidayRemarks = "Selamat Idul Fitri",
                PublicHolidayStartDate = DateTime.Now,
                PublicHolidayEndDate = DateTime.Now.AddDays(1)
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            error = _settingsService.DeletePublicHoliday(publicHolidayDTO.PublicHolidayId);

            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            // Repository is empty
            Assert.AreEqual(0, _unitOfWork.PublicHolidayRepository.CountAll());
        }

        /// <summary>
        // Test to delete a public holiday not existing
        /// </summary>
        [Test]
        public void DeletePublicHolidayNotFound()
        {
            var error = _settingsService.DeletePublicHoliday(1);

            Assert.AreEqual(ErrorCode.PUBLIC_HOLIDAY_NOT_FOUND, error);
            // Repository is empty
            Assert.AreEqual(0, _unitOfWork.PublicHolidayRepository.CountAll());
        }

        /// <summary>
        // Test to get a public holiday
        /// </summary>
        [Test]
        public void DetailsPublicHoliday()
        {
            // Public holiday from today to today + 1
            var publicHolidayDTO = new PublicHolidayDTO
            {
                PublicHolidayName = "Idul Fitri",
                PublicHolidayRemarks = "Selamat Idul Fitri",
                PublicHolidayStartDate = DateTime.Now,
                PublicHolidayEndDate = DateTime.Now.AddDays(1)
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            var publicHolidayGet = _settingsService.GetPublicHoliday(publicHolidayDTO.PublicHolidayId);

            Assert.AreEqual(publicHolidayDTO.PublicHolidayId, publicHolidayGet.PublicHolidayId);
            Assert.AreEqual(publicHolidayDTO.PublicHolidayName, publicHolidayGet.PublicHolidayName);
            Assert.AreEqual(publicHolidayDTO.PublicHolidayStartDate.Date, publicHolidayGet.PublicHolidayStartDate);
            Assert.AreEqual(publicHolidayDTO.PublicHolidayEndDate.Date, publicHolidayGet.PublicHolidayEndDate);
            Assert.AreEqual(publicHolidayDTO.PublicHolidayRemarks, publicHolidayGet.PublicHolidayRemarks);

        }

        /// <summary>
        // Test to get a public holiday not existing
        /// </summary>
        [Test]
        public void DetailsPublicHolidayNotFound()
        {
            var publicHolidayGet = _settingsService.GetPublicHoliday(1);
            Assert.AreEqual(publicHolidayGet, null);
        }

        /// <summary>
        // Test to get all public holiday
        /// </summary>
        [Test]
        public void GetAllPublicHolidays()
        {
            // Public holiday from today to today + 2
            var publicHolidayCreate1 = new PublicHolidayDTO
            {
                PublicHolidayName = "My Holiday 1",
                PublicHolidayRemarks = "My remarks 1",
                PublicHolidayStartDate = DateTime.Now.AddDays(0),
                PublicHolidayEndDate = DateTime.Now.AddDays(2)
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayCreate1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.PublicHolidayRepository.CountAll());

            // Public holiday from today + 3 to today + 4
            var publicHolidayCreate2 = new PublicHolidayDTO
            {
                PublicHolidayName = "My Holiday 2",
                PublicHolidayRemarks = "My remarks 2",
                PublicHolidayStartDate = DateTime.Now.AddDays(3),
                PublicHolidayEndDate = DateTime.Now.AddDays(4)
            };

            error = _settingsService.CreatePublicHoliday(ref publicHolidayCreate2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, _unitOfWork.PublicHolidayRepository.CountAll());

            // Public holiday from today + 5 to today + 6
            var publicHolidayCreate3 = new PublicHolidayDTO
            {
                PublicHolidayName = "My Holiday 3",
                PublicHolidayRemarks = "My remarks 3",
                PublicHolidayStartDate = DateTime.Now.AddDays(5),
                PublicHolidayEndDate = DateTime.Now.AddDays(6)
            };

            error = _settingsService.CreatePublicHoliday(ref publicHolidayCreate3);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(3, _unitOfWork.PublicHolidayRepository.CountAll());
            
            var publicHolidaysDTO = _settingsService.GetAllPublicHolidays();
            Assert.AreEqual(3, publicHolidaysDTO.Count());

            Assert.AreEqual(publicHolidayCreate1.PublicHolidayId, publicHolidaysDTO.ElementAt(0).PublicHolidayId);
            Assert.AreEqual(publicHolidayCreate1.PublicHolidayName, publicHolidaysDTO.ElementAt(0).PublicHolidayName);
            Assert.AreEqual(publicHolidayCreate1.PublicHolidayStartDate.Date, publicHolidaysDTO.ElementAt(0).PublicHolidayStartDate);
            Assert.AreEqual(publicHolidayCreate1.PublicHolidayEndDate.Date, publicHolidaysDTO.ElementAt(0).PublicHolidayEndDate);
            Assert.AreEqual(publicHolidayCreate1.PublicHolidayRemarks, publicHolidaysDTO.ElementAt(0).PublicHolidayRemarks);

            Assert.AreEqual(publicHolidayCreate2.PublicHolidayId, publicHolidaysDTO.ElementAt(1).PublicHolidayId);
            Assert.AreEqual(publicHolidayCreate2.PublicHolidayName, publicHolidaysDTO.ElementAt(1).PublicHolidayName);
            Assert.AreEqual(publicHolidayCreate2.PublicHolidayStartDate.Date, publicHolidaysDTO.ElementAt(1).PublicHolidayStartDate);
            Assert.AreEqual(publicHolidayCreate2.PublicHolidayEndDate.Date, publicHolidaysDTO.ElementAt(1).PublicHolidayEndDate);
            Assert.AreEqual(publicHolidayCreate2.PublicHolidayRemarks, publicHolidaysDTO.ElementAt(1).PublicHolidayRemarks);

            Assert.AreEqual(publicHolidayCreate3.PublicHolidayId, publicHolidaysDTO.ElementAt(2).PublicHolidayId);
            Assert.AreEqual(publicHolidayCreate3.PublicHolidayName, publicHolidaysDTO.ElementAt(2).PublicHolidayName);
            Assert.AreEqual(publicHolidayCreate3.PublicHolidayStartDate.Date, publicHolidaysDTO.ElementAt(2).PublicHolidayStartDate);
            Assert.AreEqual(publicHolidayCreate3.PublicHolidayEndDate.Date, publicHolidaysDTO.ElementAt(2).PublicHolidayEndDate);
            Assert.AreEqual(publicHolidayCreate3.PublicHolidayRemarks, publicHolidaysDTO.ElementAt(2).PublicHolidayRemarks);

        }



        // 4. Runs Twice; Once after Test Case 1 and Once After Test Case 2
        // Dispose Objects Used in Each Test which are no longer required
        [TearDown]
        public void RunOnceAfterEachTests()
        {

        }

        // 5. Runs Once After All of The Aformentioned Methods
        // Dispose all Mocks and Global Objects
        [TestFixtureTearDown]
        public void RunOnceAfterAll()
        {

        }



    }
}

