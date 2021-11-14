﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OtripleS.Portal.Web.Models.Courses;
using OtripleS.Portal.Web.Models.Courses.Exceptions;
using Xunit;

namespace OtripleS.Portal.Web.Tests.Unit.Services.Courses
{
    public partial class CourseServiceTests
    {
        [Theory]
        [MemberData(nameof(CriticalApiException))]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllIfCriticalDependencyExceptionOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            var failedCourseDependencyException =
                new FailedCourseDependencyException(
                    criticalDependencyException);

            var expectedCourseDependencyException =
                new CourseDependencyException(
                    failedCourseDependencyException);

            this.apiBrokerMock.Setup(broker =>
                broker.GetAllCoursesAsync())
                    .ThrowsAsync(criticalDependencyException);

            // when
            ValueTask<List<Course>> retrievedCoursesTask =
                this.courseService.RetrieveAllCoursesAsync();

            // then
            await Assert.ThrowsAsync<CourseDependencyException>(() =>
               retrievedCoursesTask.AsTask());

            this.apiBrokerMock.Verify(broker =>
                broker.GetAllCoursesAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedCourseDependencyException))),
                        Times.Once);

            this.apiBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
