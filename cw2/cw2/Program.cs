using System;
using static LiquidContainer;

public interface IContainer
{
    void UnloadCargo();
    void LoadCargo(Cargo cargo);
}

public interface IHazardNotifier
{
    void NotifyHazard(string containerNumber); 
}

public class OverfillException : Exception
{
    public OverfillException(string message) : base(message)
    {
    }
}

public class Harbor
{
    private List<Ship> shipList;

 
    public Harbor()
    {
        shipList = new List<Ship>();
    }

    
    public void DockShip(Ship ship)
    {
        shipList.Add(ship);
        Console.WriteLine($"Ship {ship} docked successfully.");
    }


    public void UndockShip(Ship ship)
    {
        if (shipList.Contains(ship))
        {
            shipList.Remove(ship);
            Console.WriteLine($"Ship {ship} undocked successfully.");
        }
        else
        {
            Console.WriteLine($"Ship {ship} is not docked in this harbor.");
        }
    }
}
public class Ship
{
    private List<AbstractContainer> containers;
    private double maxSpeed;
    private double maxContainersNumber;
    private double maxContainersWeight;

    public Ship(double maxSpeed, double maxContainersNumber, double maxContainersWeight)
    {
        this.containers = new List<AbstractContainer>();
        this.maxSpeed = maxSpeed;
        this.maxContainersNumber = maxContainersNumber;
        this.maxContainersWeight = maxContainersWeight;
    }

    public void AddContainer(AbstractContainer container)
    {
      
        if (containers.Count < maxContainersNumber)
        {
          
            if (GetTotalContainersWeight() + container.GetCurrentCargoWeight() <= maxContainersWeight)
            {
                containers.Add(container);
            }
            else
            {
                throw new Exception("Adding container would exceed maximum containers weight on the ship.");
            }
        }
        else
        {
            throw new Exception("Adding container would exceed maximum containers number on the ship.");
        }
    }

    public void AddContainers(List<AbstractContainer> containersToAdd)
    {
        
        if (containers.Count + containersToAdd.Count <= maxContainersNumber)
        {
            double totalWeight = GetTotalContainersWeight();
       
            foreach (AbstractContainer container in containersToAdd)
            {
                totalWeight += container.GetCurrentCargoWeight();
                if (totalWeight > maxContainersWeight)
                {
                    throw new Exception("Adding containers would exceed maximum containers weight on the ship.");
                }
            }

         
            containers.AddRange(containersToAdd);
        }
        else
        {
            throw new Exception("Adding containers would exceed maximum containers number on the ship.");
        }
    }

    public void RemoveContainer(string serialNumber)
    {
        AbstractContainer containerToRemove = containers.Find(container => container.GetSerialNumber() == serialNumber);
        if (containerToRemove != null)
        {
            containers.Remove(containerToRemove);
        }
        else
        {
            throw new Exception("Container with the specified serial number not found on the ship.");
        }
    }

    public double GetTotalContainersWeight()
    {
        double totalWeight = 0;
        foreach (AbstractContainer container in containers)
        {
            totalWeight += container.GetCurrentCargoWeight();
        }
        return totalWeight;
    }

    public void PrintShipInfo()
    {
        Console.WriteLine($"Max Speed: {maxSpeed}");
        Console.WriteLine($"Max Containers Number: {maxContainersNumber}");
        Console.WriteLine($"Max Containers Weight: {maxContainersWeight}");
        Console.WriteLine("Containers:");
        foreach (AbstractContainer container in containers)
        {

            Console.WriteLine(container.PrintContainerInfo());
        }
    }
}

public class Cargo
{
    private string name;
    private int weight;
    public Cargo(string name, int weight)
    {
        this.name = name;
        this.weight = weight;
    }

    public string getName()
    {
        return name;
    }
    public int getWeight()
    {
        return this.weight;
    }
}

public class LiquidCargo : Cargo
{
    private bool isDangerous;

    public LiquidCargo(string name, int weight, bool isDangerous) : base(name, weight)
    {
        this.isDangerous = isDangerous;
    }

    public bool IsDangerous
    {
        get { return isDangerous; }
    }

}

public class ColdCargo : Cargo
{
    private double tempNeeded;

    public ColdCargo(string name, int weight, double tempNeeded) : base(name, weight)
    {
        this.tempNeeded = tempNeeded;
    }

    public double TempNeeded
    {
        get { return tempNeeded; }
    }
}

public abstract class AbstractContainer : IContainer
{
    protected Cargo cargo;
    protected double currentCargoWeight;
    protected double height;
    protected double weight;
    protected double depht;
    protected string serialNumber;
    protected double maxCapacity;
    protected string containerType;

    public AbstractContainer(double maxCapacity, string containerType)
    {
        this.serialNumber = GenerateSerialNumber(containerType);
        this.maxCapacity = maxCapacity;
        this.containerType = containerType;
    }

    private static Dictionary<string, int> serialNumbers = new Dictionary<string, int>();


    private string GenerateSerialNumber(string containerType)
    {
        int number;
        if (serialNumbers.ContainsKey(containerType))
        {
            
            number = serialNumbers[containerType] + 1;
            serialNumbers[containerType] = number;
        }
        else
        {
            
            number = 1;
            serialNumbers.Add(containerType, number);
        }

        return "KON-" + containerType + "-" + number.ToString("D4"); 
    }

    public virtual void UnloadCargo()
    {
        this.cargo = null;

    }

    public virtual void LoadCargo(Cargo cargo)

    {
        if (cargo.getWeight() > maxCapacity)
        {
            throw new OverfillException("Cargo weight exceeds container capacity");
        }
        this.cargo = cargo;
        this.currentCargoWeight = cargo.getWeight();
    }

    public double GetCurrentCargoWeight()
    {
        return currentCargoWeight;
    }

    public string GetSerialNumber()
    {
        return serialNumber;
    }

    public string PrintContainerInfo()
    {
        string containerInfo = $"Serial Number: {serialNumber}\n" +
                               $"Container Type: {containerType}\n" +
                               $"Current Cargo Weight: {currentCargoWeight}\n" +
                               $"Height: {height}\n" +
                               $"Weight: {weight}\n" +
                               $"Depth: {depht}\n" +
                               $"Max Capacity: {maxCapacity}\n";
        if (cargo != null)
        {
            containerInfo += $"Cargo Name: {cargo.getName()}\n" +
                             $"Cargo Weight: {cargo.getWeight()}\n";
        }
        else
        {
            containerInfo += "No cargo loaded\n";
        }

        return containerInfo;
    }
}

public class LiquidContainer : AbstractContainer, IHazardNotifier
{
    private static string CONTAINER_TYPE = "L";

    public LiquidContainer(double maxCapacity) : base(maxCapacity, CONTAINER_TYPE)
    {
    }

    public override void LoadCargo(Cargo cargo)
    {
        base.LoadCargo(cargo);

        if (cargo is LiquidCargo liquidCargo)

        {
            if (liquidCargo.getWeight() > maxCapacity)
            {
                throw new OverfillException("Cargo weight exceeds container capacity");
            }

            
            if (IsHazardousCargo(liquidCargo))
            {
              
                if (liquidCargo.getWeight() > maxCapacity * 0.5)
                {
                    throw new OverfillException("Hazardous cargo cannot exceed 50% of container capacity");
                }
            }
            else
            {
                
                if (liquidCargo.getWeight() > maxCapacity * 0.9)
                {
                    throw new OverfillException("Cargo weight cannot exceed 90% of container capacity");
                }
            }
        }
        else
        {
            throw new ArgumentException("Cannot load non-liquid cargo into liquid container");
        }
    }

    public override void UnloadCargo()
    {
        base.UnloadCargo();
        this.currentCargoWeight = 0;
    }
    private bool IsHazardousCargo(LiquidCargo cargo)
    {
        if (cargo.IsDangerous)
        {
            return true;
        }
        else return false;
    }

    public void NotifyHazard(string containerNumber)
    {
        Console.WriteLine($"Hazardous situation detected in container {containerNumber}. Please take necessary precautions.");
    }
}


public class GasContainer : AbstractContainer, IHazardNotifier
{
    private double pressure;
    private static string CONTAINER_TYPE = "G";

    public GasContainer(double maxCapacity) : base(maxCapacity, CONTAINER_TYPE)
    {
    }

    public void NotifyHazard(string containerNumber)
    {
        Console.WriteLine($"Dangerous situation detected in gas container {containerNumber}. Please take necessary precautions.");
    }


   
    public override void UnloadCargo()
    {
  
        currentCargoWeight *= 0.05;
        base.UnloadCargo();
    }

    public void SetPressure(double pressure)
    {
        this.pressure = pressure;
    }
}


public class RefrigeratedContainer : AbstractContainer, IHazardNotifier
{
    private Cargo cargoType;
    private double currentTemp;
    private static string CONTAINER_TYPE = "C";

    public RefrigeratedContainer(double maxCapacity, Cargo cargoType, double requiredTemperature) : base(maxCapacity, CONTAINER_TYPE)
    {
        this.cargoType = cargoType;
        this.currentTemp = requiredTemperature;
    }


    public void NotifyHazard(string containerNumber)
    {
        Console.WriteLine($"Dangerous situation detected in refrigerated container {containerNumber}. Please take necessary precautions.");
    }


    public override void LoadCargo(Cargo cargo)
    {
        if (cargo is ColdCargo coldCargo)
        {

            if (cargo != cargoType)
            {
                throw new Exception("Cannot load this product type to this container");
            }

            if (currentTemp < coldCargo.TempNeeded)
            {
                throw new Exception("Invalid required temperature");
            }

            if (cargo.getWeight() > maxCapacity)
            {
                throw new OverfillException("Cargo weight exceeds container capacity");
            }
        }
        else
        {
            throw new ArgumentException("Cannot laod this cargo type");
        }
        base.LoadCargo(cargo);

    }

    public static void Main(string[] args)
    {

        Harbor harbor = new Harbor();

        Ship shipUno = new Ship(50, 100, 100000); 
        Ship shipSecundo = new Ship(20, 120, 200000);


        harbor.DockShip(shipUno);
        harbor.DockShip(shipSecundo);


        LiquidContainer liquidContainer = new LiquidContainer(10000);
        LiquidCargo milk = new LiquidCargo("milk", 8000, false);


        GasContainer gasContainer = new GasContainer(5000);
        Cargo propan = new Cargo("propan", 4000);

        gasContainer.LoadCargo(propan);
        liquidContainer.LoadCargo(milk);


        shipUno.AddContainer(liquidContainer);

        List<AbstractContainer> containersToAdd = new List<AbstractContainer>();
        containersToAdd.Add(liquidContainer);
        containersToAdd.Add(gasContainer);

        shipUno.AddContainers(containersToAdd);

        Console.WriteLine(shipUno.GetTotalContainersWeight());

        liquidContainer.UnloadCargo();

        Console.WriteLine(liquidContainer.GetCurrentCargoWeight());

        shipUno.RemoveContainer(liquidContainer.GetSerialNumber());

        Console.WriteLine(shipUno.GetTotalContainersWeight());

        shipUno.PrintShipInfo();

      
    }
}

