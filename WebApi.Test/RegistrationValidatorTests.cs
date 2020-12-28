﻿using DataAccessLibrary.DB;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WEBApi.DTOs;
using WEBApi.Validators;
using Xunit;
using FluentValidation.Results;

namespace WebApi.Tests
{
    public class RegistrationValidatorTests
    {
        private readonly RegistrationValidator _systemUnderTesting;
        private readonly Mock<IUserReadRepository> _repoMock = new Mock<IUserReadRepository>();
        public RegistrationValidatorTests()
        {
            _systemUnderTesting = new(_repoMock.Object);
        }
        [Theory]
        [InlineData("Email","username","password123")]
        [InlineData("Email@gmail.com", "u", "password123")]
        [InlineData("Email@gmail.com", "username", "pass")]
        public async Task ValidateAsync_ShouldFail_BadInput(string email, string nickname, string password)
        {
            //arrange
            UserRegistrationModel model = new()
            {
                Email = email,
                Nickname = nickname,
                Password = password
            };
            //act
            //user is assumed not to present in the db
            _repoMock.Setup(x => x.CheckIsEmailPresent(email, CancellationToken.None)).ReturnsAsync(false);

            ValidationResult result =  await _systemUnderTesting.ValidateAsync(model, CancellationToken.None);
            //assert
            Assert.False(result.IsValid);
        }
        [Theory]
        [InlineData("Email@gmail.com", "username", "password123")]
        [InlineData("theruslan.prog@gmail.com", "RuslanPr0g", "Tasman2020")]
        public async Task ValidateAsync_ShouldFail_NotAvailableEmail(string email, string nickname, string password)
        {
            //arrange
            UserRegistrationModel model = new()
            {
                Email = email,
                Nickname = nickname,
                Password = password
            };
            //act
            //user is assumed to be present in the db
            _repoMock.Setup(x => x.CheckIsEmailPresent(email, CancellationToken.None)).ReturnsAsync(true);

            ValidationResult result = await _systemUnderTesting.ValidateAsync(model, CancellationToken.None);
            //assert
            Assert.False(result.IsValid);
        }
        [Theory]
        [InlineData("Email@gmail.com", "username", "password123")]
        [InlineData("theruslan.prog@gmail.com", "RuslanPr0g", "Tasman2020")]
        public async Task ValidateAsync_ShouldWork_ValidData(string email, string nickname, string password)
        {
            //arrange
            UserRegistrationModel model = new()
            {
                Email = email,
                Nickname = nickname,
                Password = password
            };
            //act
            //user is assumed not to present in the db
            _repoMock.Setup(x => x.CheckIsEmailPresent(email, CancellationToken.None)).ReturnsAsync(false);

            ValidationResult result = await _systemUnderTesting.ValidateAsync(model, CancellationToken.None);
            //assert
            Assert.True(result.IsValid);
        }
    }
}
