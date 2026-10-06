namespace SmartWarehouse.Core;

public class WarehouseSensor
{
    public string SensorId { get; private set; }
    public string LocationTag { get; private set; }
    public double CurrentTemperature { get; private set; } = 0.0;
    public bool IsActive { get; private set; } = false;
    public bool IsAlertTriggered { get; private set; } = false;
    public double CriticalThresholdCelsius { get; private set; } = 4.0;

    public WarehouseSensor(string sensorId, string locationTag, double criticalThreshold = 4.0)
    {
        if (string.IsNullOrWhiteSpace(sensorId))
        {
            throw new ArgumentException("SensorId cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(locationTag))
        {
            throw new ArgumentException("LocationTag cannot be null, empty, or whitespace.");
        }

        SensorId = sensorId;
        LocationTag = locationTag;
        CriticalThresholdCelsius = criticalThreshold;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsAlertTriggered = false;
    }

    public void RecordReading(double newTemperature)
    {
        if (IsActive == false)
        {
            throw new InvalidOperationException("Cannot record a reading while the sensor is inactive.");
        }

        if (newTemperature < -50.0 || newTemperature > 80.0)
        {
            throw new ArgumentOutOfRangeException("newTemperature", "Temperature must be between -50.0 and 80.0.");
        }

        CurrentTemperature = newTemperature;
        CheckAlert();
    }

    public void UpdateThreshold(double newThreshold)
    {
        if (newThreshold < -30.0 || newThreshold > 50.0)
        {
            throw new ArgumentOutOfRangeException("newThreshold", "Threshold must be between -30.0 and 50.0.");
        }

        CriticalThresholdCelsius = newThreshold;
        CheckAlert();
    }

    private void CheckAlert()
    {
        IsAlertTriggered = CurrentTemperature >= CriticalThresholdCelsius;
    }
}
