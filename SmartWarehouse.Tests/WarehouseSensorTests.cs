using SmartWarehouse.Core;

namespace SmartWarehouse.Tests;

[TestClass]
public sealed class WarehouseSensorTests
{
    [TestMethod]
    public void Constructor_ValidArguments_SetsInitialState()
    {
        // Arrange
        string sensorId = "SENSOR-1";
        string locationTag = "Cold-Vault-01";

        // Act
        WarehouseSensor sensor = new WarehouseSensor(sensorId, locationTag);

        // Assert
        Assert.AreEqual("SENSOR-1", sensor.SensorId);
        Assert.AreEqual("Cold-Vault-01", sensor.LocationTag);
        Assert.AreEqual(0.0, sensor.CurrentTemperature);
        Assert.AreEqual(false, sensor.IsActive);
        Assert.AreEqual(false, sensor.IsAlertTriggered);
        Assert.AreEqual(4.0, sensor.CriticalThresholdCelsius);
    }

    [TestMethod]
    public void Constructor_CustomThreshold_StoresThreshold()
    {
        // Arrange
        double threshold = 10.5;

        // Act
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01", threshold);

        // Assert
        Assert.AreEqual(10.5, sensor.CriticalThresholdCelsius);
    }

    [TestMethod]
    public void Constructor_NullSensorId_ThrowsArgumentException()
    {
        // Arrange
        string locationTag = "Cold-Vault-01";

        // Act
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => new WarehouseSensor(null, locationTag));

        // Assert
        Assert.AreEqual("SensorId cannot be null, empty, or whitespace.", exception.Message);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_BlankSensorId_ThrowsArgumentException(string sensorId)
    {
        // Arrange
        string locationTag = "Cold-Vault-01";

        // Act
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => new WarehouseSensor(sensorId, locationTag));

        // Assert
        Assert.AreEqual("SensorId cannot be null, empty, or whitespace.", exception.Message);
    }

    [TestMethod]
    public void Constructor_NullLocationTag_ThrowsArgumentException()
    {
        // Arrange
        string sensorId = "SENSOR-1";

        // Act
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => new WarehouseSensor(sensorId, null));

        // Assert
        Assert.AreEqual("LocationTag cannot be null, empty, or whitespace.", exception.Message);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_BlankLocationTag_ThrowsArgumentException(string locationTag)
    {
        // Arrange
        string sensorId = "SENSOR-1";

        // Act
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => new WarehouseSensor(sensorId, locationTag));

        // Assert
        Assert.AreEqual("LocationTag cannot be null, empty, or whitespace.", exception.Message);
    }

    [TestMethod]
    public void Activate_InactiveSensor_SetsIsActiveTrue()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");

        // Act
        sensor.Activate();

        // Assert
        Assert.AreEqual(true, sensor.IsActive);
    }

    [TestMethod]
    public void Activate_CalledTwice_StaysActive()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");

        // Act
        sensor.Activate();
        sensor.Activate();

        // Assert
        Assert.AreEqual(true, sensor.IsActive);
    }

    [TestMethod]
    public void Deactivate_ActiveSensor_SetsIsActiveFalse()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();

        // Act
        sensor.Deactivate();

        // Assert
        Assert.AreEqual(false, sensor.IsActive);
    }

    [TestMethod]
    public void Deactivate_SensorInAlert_ResetsAlert()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();
        sensor.RecordReading(10.0);

        // Act
        sensor.Deactivate();

        // Assert
        Assert.AreEqual(false, sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void RecordReading_InactiveSensor_ThrowsInvalidOperationException()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");

        // Act
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => sensor.RecordReading(2.0));

        // Assert
        Assert.AreEqual("Cannot record a reading while the sensor is inactive.", exception.Message);
        Assert.AreEqual(0.0, sensor.CurrentTemperature);
    }

    [TestMethod]
    public void RecordReading_ActiveSensor_UpdatesCurrentTemperature()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();

        // Act
        sensor.RecordReading(2.5);

        // Assert
        Assert.AreEqual(2.5, sensor.CurrentTemperature);
    }

    [DataTestMethod]
    [DataRow(-10.0, false)]
    [DataRow(3.9, false)]
    [DataRow(4.0, true)]
    [DataRow(4.1, true)]
    [DataRow(25.0, true)]
    public void RecordReading_DefaultThreshold_SetsAlertState(double temperature, bool expectedAlert)
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();

        // Act
        sensor.RecordReading(temperature);

        // Assert
        Assert.AreEqual(expectedAlert, sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void RecordReading_TemperatureDropsBelowThreshold_ClearsAlert()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();
        sensor.RecordReading(8.0);

        // Act
        sensor.RecordReading(2.0);

        // Assert
        Assert.AreEqual(false, sensor.IsAlertTriggered);
    }

    [DataTestMethod]
    [DataRow(-50.0)]
    [DataRow(80.0)]
    public void RecordReading_BoundaryValue_IsAccepted(double temperature)
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();

        // Act
        sensor.RecordReading(temperature);

        // Assert
        Assert.AreEqual(temperature, sensor.CurrentTemperature);
    }

    [DataTestMethod]
    [DataRow(-50.1)]
    [DataRow(80.1)]
    public void RecordReading_OutsideBoundary_ThrowsArgumentOutOfRangeException(double temperature)
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();
        sensor.RecordReading(3.0);

        // Act
        ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sensor.RecordReading(temperature));

        // Assert
        Assert.AreEqual("newTemperature", exception.ParamName);
        Assert.AreEqual(3.0, sensor.CurrentTemperature);
    }

    [DataTestMethod]
    [DataRow(-30.0)]
    [DataRow(0.0)]
    [DataRow(50.0)]
    public void UpdateThreshold_ValidValue_UpdatesThreshold(double threshold)
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");

        // Act
        sensor.UpdateThreshold(threshold);

        // Assert
        Assert.AreEqual(threshold, sensor.CriticalThresholdCelsius);
    }

    [DataTestMethod]
    [DataRow(-30.1)]
    [DataRow(50.1)]
    public void UpdateThreshold_OutsideRange_ThrowsArgumentOutOfRangeException(double threshold)
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");

        // Act
        ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sensor.UpdateThreshold(threshold));

        // Assert
        Assert.AreEqual("newThreshold", exception.ParamName);
        Assert.AreEqual(4.0, sensor.CriticalThresholdCelsius);
    }

    [TestMethod]
    public void UpdateThreshold_LoweredBelowCurrentTemperature_TriggersAlert()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01", 10.0);
        sensor.Activate();
        sensor.RecordReading(6.0);

        // Act
        sensor.UpdateThreshold(5.0);

        // Assert
        Assert.AreEqual(true, sensor.IsAlertTriggered);
    }

    [TestMethod]
    public void UpdateThreshold_RaisedAboveCurrentTemperature_ClearsAlert()
    {
        // Arrange
        WarehouseSensor sensor = new WarehouseSensor("SENSOR-1", "Cold-Vault-01");
        sensor.Activate();
        sensor.RecordReading(6.0);

        // Act
        sensor.UpdateThreshold(10.0);

        // Assert
        Assert.AreEqual(false, sensor.IsAlertTriggered);
    }
}
