using System;
using Api.Models;
using Xunit;
namespace Api.Tests
{
    public class RunMapPointUpdateTests
    {
        [Fact]
        public void CheckSetValues()
        {
            //Arrange
            Run run = new Run();
            MapPoint[] newRoute = {
                new MapPoint(0,0,0),
                new MapPoint(10,10,10)
            };

            //Act
            run.Points = newRoute;

            //Assert
            Assert.Equal(10u, run.ElevationDelta);
            Assert.NotEqual(0u, run.DistanceTotal);
        }

        [Fact]
        public void CheckDataIntegrity()
        {
            //Arrange
            Run run = new Run();
            MapPoint[] newRoute = {
                new MapPoint(0,0,0),
                new MapPoint(10,10,10)
            };

            //Act
            run.Points = newRoute;

            //Assert
            Assert.Equal(0, BitConverter.ToDouble(run.RawPoints[0..8]));
            Assert.Equal(0, BitConverter.ToDouble(run.RawPoints[8..16]));
            Assert.Equal(0, BitConverter.ToDouble(run.RawPoints[16..24]));


            Assert.Equal(10, BitConverter.ToDouble(run.RawPoints[24..32]));
            Assert.Equal(10, BitConverter.ToDouble(run.RawPoints[32..40]));
            Assert.Equal(10, BitConverter.ToDouble(run.RawPoints[40..48]));
        }

        [Fact]
        public void CheckSetValuesNull()
        {
            //Arrange
            Run run = new Run();
            MapPoint[] newRoute = null;

            //Act
            run.Points = newRoute;

            //Assert
            Assert.Equal(0u, run.ElevationDelta);
            Assert.Equal(0u, run.DistanceTotal);
        }
    }
}