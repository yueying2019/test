using System.Collections;
using System.Collections.Generic;
using System;
namespace OpenComputer
{
    class Main_Test
    {
        /// <summary>
        /// 测试主函数
        /// </summary>
        static void Main()
        {
            //电脑实例化
            Computer Com = new Computer("Computer1", "Computer");
            //待机状态
            Com.WaitForOpen();

            //尝试开机
            if (!Com.Opening())
            {
                //开机失败
                Console.WriteLine("The Computer can't be opened!");
                Console.WriteLine("Unfortunately! OMG!");
            }
        }
    }

    /// <summary>
    /// 电脑设备父类接口，设置和读取设备序列号和设备类型
    /// </summary>
    public interface IComputerSerial
    {
        #region 公有属性
        /// <summary>
        /// 序列号属性
        /// </summary>
        /// <value>只读</value>
        string SerialNumber { get; }
        /// <summary>
        /// 设备类型属性
        /// </summary>
        /// <value>只读</value>
        string DeviceType { get; }
        #endregion
    }

    /// <summary>
    /// 电脑设备父类，设置和读取设备序列号和设备类型
    /// </summary>
    public class ComputerSerial : IComputerSerial
    {
        #region 保护字段
        /// <summary>
        /// 序列号
        /// </summary>
        protected string serialNumber;
        /// <summary>
        /// 设备类型
        /// </summary>
        protected string deviceType;
        #endregion

        #region 公有属性
        /// <summary>
        /// 序列号属性
        /// </summary>
        /// <value>只读</value>
        public string SerialNumber
        {
            get
            {
                return serialNumber;
            }
        }

        /// <summary>
        /// 设备类型属性
        /// </summary>
        /// <value>只读</value>
        public string DeviceType
        {
            get
            {
                return deviceType;
            }
        }
        #endregion
    }

    /// <summary>
    /// 电脑接口，包含主机和显示器
    /// </summary>
    public interface IComputer : IComputerSerial
    {
        #region 公有属性
        /// <summary>
        /// 主机属性
        /// </summary>
        /// <value>只读</value>
        IHost ComputerHost { get; }
        /// <summary>
        /// 显示器属性
        /// </summary>
        /// <value>只读</value>
        IMonitor ComputerMonitor { get; }
        #endregion

        #region 公有方法
        /// <summary>
        /// 待机函数
        /// </summary>
        void WaitForOpen();

        /// <summary>
        /// 开机函数
        /// </summary>
        /// <returns>开机是否成功</returns>
        bool Opening();

        /// <summary>
        /// 电脑上电待机，初始化主机
        /// </summary>
        void ComputerStandBy();

        /// <summary>
        /// 电脑上电开机，初始化显示器，且主机向其它设备传播电源
        /// </summary>
        /// <returns>CPU是否存在</returns>
        bool ComputerPowerOn();
        #endregion
    }

    /// <summary>
    /// 电脑类，继承电脑设备父类，实现电脑接口
    /// 包含主机和显示器
    /// </summary>
    public class Computer : ComputerSerial, IComputer
    {
        private Host host;
        private Monitor monitor;

        #region 公有属性
        /// <summary>
        /// 主机属性
        /// </summary>
        /// <value>只读</value>
        public IHost ComputerHost
        {
            get
            {
                return host;
            }
        }

        /// <summary>
        /// 显示器属性
        /// </summary>
        /// <value>只读</value>
        public IMonitor ComputerMonitor
        {
            get
            {
                return monitor;
            }
        }
        #endregion

        /// <summary>
        /// 电脑的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public Computer(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
        }

        #region 公有方法
        /// <summary>
        /// 待机函数，通小电流，电脑待机
        /// </summary>
        public void WaitForOpen()
        {
            //待机
            ComputerStandBy();
            //电压大于0V且小于10V，处于通电待机状态
            if (ComputerHost.HostPowerSupply.StandBy() > 0 && ComputerHost.HostPowerSupply.StandBy() < 10)
            {
                Console.WriteLine("Computer is standbying!\n");
            }
        }

        /// <summary>
        /// 开机函数，CPU先自检，然后依次检查各个设备
        /// </summary>
        /// <returns>开机是否成功</returns>
        public bool Opening()
        {
            //电压大于180V且小于250V，可以开机
            if (ComputerHost.HostPowerSupply.PowerOn() > 180 && ComputerHost.HostPowerSupply.PowerOn() < 250)
            {
                Console.WriteLine("Computer is opening!\n");

                //向其它设备供电，并检测CPU是否存在
                if (!ComputerPowerOn())
                {
                    return false;
                }

                //CPU自检
                if (!ComputerHost.HostMainBoard.MainBoardCPU.TestCPUSelf())
                {
                    return false;
                }

                //CPU检查内存是否存在，并检测内存属性
                if (!ComputerHost.HostMainBoard.MainBoardCPU.TestMemoryList(ComputerHost.HostMainBoard.MainBoardMemoryList))
                {
                    return false;
                }

                //CPU检查显卡是否存在，并检测显卡属性
                if (!ComputerHost.HostMainBoard.MainBoardCPU.TestGraphicsCardList(ComputerHost.HostMainBoard.MainBoardGraphicsCardList))
                {
                    return false;
                }
                else
                {
                    foreach (GraphicsCard graphicsCard in ComputerHost.HostMainBoard.MainBoardGraphicsCardList)
                    {
                        //显卡正常，显示器显示主显卡传递过来的显卡信息
                        ComputerMonitor.ShowMessage("GraphicsCard: SerialNumber: " +
                            ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetSerialNumber(graphicsCard.SerialNumber) +
                            ", DeviceType: " + ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetDeviceType(graphicsCard.DeviceType));
                    }
                    Console.WriteLine();
                    //显示器显示主显卡传递过来的CPU设备信息
                    ComputerMonitor.ShowMessage("CPU: SerialNumber: " +
                        ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetSerialNumber(ComputerHost.HostMainBoard.MainBoardCPU.SerialNumber) +
                        ", DeviceType: " + ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetDeviceType(ComputerHost.HostMainBoard.MainBoardCPU.DeviceType) + "\n");
                }

                //CPU检查硬盘是否存在，并检测硬盘属性
                if (!ComputerHost.HostMainBoard.MainBoardCPU.TestHardDiskList(ComputerHost.HostMainBoard.MainBoardHardDiskList))
                {
                    return false;
                }
                else
                {
                    foreach (HardDisk hardDisk in ComputerHost.HostMainBoard.MainBoardHardDiskList)
                    {
                        //硬盘正常，显示器显示主显卡传递过来的硬盘信息
                        ComputerMonitor.ShowMessage("HardDisk: SerialNumber: " +
                            ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetSerialNumber(hardDisk.SerialNumber) +
                            ", DeviceType: " + ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetDeviceType(hardDisk.DeviceType));
                    }
                }
                Console.WriteLine();
                //CPU依次检查各USB设备，但USB设备出错不会影响开机
                ComputerHost.HostMainBoard.MainBoardCPU.TestUSBList(ComputerHost.HostMainBoard.MainBoardUSBList);
            }
            else if (ComputerHost.HostPowerSupply.PowerOn() >= 250)
            {
                //电压过大，电脑烧坏
                Console.WriteLine("Power is so big! The computer has dead!");
                return false;
            }
            else
            {
                //电压过小，无法正常开机
                Console.WriteLine("Power is so small! The computer can't be open!");
                return false;
            }

            //开机成功
            Console.WriteLine("The Computer have opened!");
            //显示器显示主显卡传递过来的电脑信息
            ComputerMonitor.ShowMessage("Computer: SerialNumber: " +
                ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetSerialNumber(this.SerialNumber) +
                ", DeviceType: " + ComputerHost.HostMainBoard.MainBoardGraphicsCardList[0].GetDeviceType(this.DeviceType) + "\n");
            Console.WriteLine("Congratulation! Hello World!");
            return true;
        }

        /// <summary>
        /// 电脑上电待机，初始化主机
        /// </summary>
        public void ComputerStandBy()
        {
            //主机初始化
            host = new Host("Host1", "Host");
        }

        /// <summary>
        /// 电脑上电开机，初始化显示器，且主机向其它设备传播电源
        /// </summary>
        /// <returns>CPU是否存在</returns>
        public bool ComputerPowerOn()
        {
            //显示器初始化
            monitor = new Monitor("Monitor1", "Monitor");
            //主机向其它设备传播电源
            host.HostPowerOn();

            if (host.HostMainBoard.MainBoardCPU != null)
            {
                return true;
            }
            Console.WriteLine("CPU does not exist!!!\n");
            return false;
        }
        #endregion
    }

    /// <summary>
    /// 电源接口
    /// </summary>
    public interface IPowerSupply : IComputerSerial
    {
        #region 公有属性
        /// <summary>
        /// 待机电压
        /// </summary>
        /// <value>只读</value>
        double StandByVoltage { get; }
        /// <summary>
        /// 正常电压
        /// </summary>
        /// <value>只读</value>
        double NormalVoltage { get; }
        #endregion

        #region 公有方法
        /// <summary>
        /// 待机供电
        /// </summary>
        /// <returns>返回待机电压</returns>
        double StandBy();

        /// <summary>
        /// 开机供电
        /// </summary>
        /// <returns>返回正常电压</returns>
        double PowerOn();
        #endregion
    }

    /// <summary>
    /// 电源类，继承电脑设备父类，实现电源接口
    /// </summary>
    public class PowerSupply : ComputerSerial, IPowerSupply
    {
        private double standbyVoltage;
        private double normalVoltage;

        #region 公有属性
        /// <summary>
        /// 待机电压
        /// </summary>
        /// <value>只读</value>
        public double StandByVoltage
        {
            get
            {
                return standbyVoltage;
            }
        }

        /// <summary>
        /// 正常电压
        /// </summary>
        /// <value>只读</value>
        public double NormalVoltage
        {
            get
            {
                return normalVoltage;
            }
        }
        #endregion

        /// <summary>
        /// 电源的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        /// <param name="standbyVoltage">待机电压</param>
        /// <param name="normalVoltage">正常电压</param>
        public PowerSupply(string serialNumber, string deviceType, double standbyVoltage, double normalVoltage)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
            this.standbyVoltage = standbyVoltage;
            this.normalVoltage = normalVoltage;
        }

        #region 公有方法
        /// <summary>
        /// 待机供电
        /// </summary>
        /// <returns>返回待机电压</returns>
        public double StandBy()
        {
            return this.StandByVoltage;
        }

        /// <summary>
        /// 开机供电
        /// </summary>
        /// <returns>返回正常电压</returns>
        public double PowerOn()
        {
            return this.NormalVoltage;
        }
        #endregion
    }

    /// <summary>
    /// CPU接口
    /// </summary>
    public interface ICPU : IComputerSerial
    {
        #region 公有方法
        /// <summary>
        /// CPU自检函数
        /// </summary>
        /// <returns>CPU是否正常</returns>
        bool TestCPUSelf();

        /// <summary>
        /// 内存检查函数
        /// </summary>
        /// <param name="MemoryList">内存列表</param>
        /// <returns>内存是否正常</returns>
        bool TestMemoryList(List<Memory> MemoryList);

        /// <summary>
        /// 显卡检查函数
        /// </summary>
        /// <param name="GraphicsCardList">显卡列表</param>
        /// <returns>显卡是否正常</returns>
        bool TestGraphicsCardList(List<GraphicsCard> GraphicsCardList);

        /// <summary>
        /// 硬盘检查函数
        /// </summary>
        /// <param name="HardDiskList">硬盘列表</param>
        /// <returns>硬盘是否正常</returns>
        bool TestHardDiskList(List<HardDisk> HardDiskList);

        /// <summary>
        /// USB设备检查函数
        /// </summary>
        /// <param name="UsbList">USB设备列表</param>
        /// <returns>USB设备是否正常</returns>
        void TestUSBList(List<USB> UsbList);

        /// <summary>
        /// 数据运算
        /// </summary>
        /// <param name="Data">数据列表</param>
        void OperateData(List<byte> Data);

        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="Data">数据列表</param>
        void ProcessData(List<byte> Data);
        #endregion
    }

    /// <summary>
    /// CPU类，继承电脑设备父类，实现CPU接口
    /// </summary>
    public class CPU : ComputerSerial, ICPU
    {
        /// <summary>
        /// CPU的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public CPU(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
        }

        #region 公有方法
        /// <summary>
        /// CPU自检函数
        /// </summary>
        /// <returns>CPU是否正常</returns>
        public bool TestCPUSelf()
        {
            //检查序列号和设备类型
            if (this.SerialNumber != null && this.DeviceType == "CPU")
            {
                Console.WriteLine("CPU is normal!\n");
                return true;
            }
            else
            {
                Console.WriteLine("CPU is bad!\n");
                return false;
            }
        }

        /// <summary>
        /// 内存检查函数
        /// </summary>
        /// <param name="MemoryList">内存列表</param>
        /// <returns>内存是否正常</returns>
        public bool TestMemoryList(List<Memory> MemoryList)
        {
            //内存不存在
            if (MemoryList == null)
            {
                Console.WriteLine("MemoryList does not exist!!!\n");
                return false;
            }
            //检测内存属性
            int j = 0;
            foreach (Memory memory in MemoryList)
            {
                if (memory == null)
                {
                    continue;
                }
                else if (memory.SerialNumber != null && memory.DeviceType == "Memory")
                {
                    Console.WriteLine(memory.SerialNumber + " is normal!");
                    j++;
                }
                else
                {
                    //内存条损坏，无法开机
                    Console.WriteLine(memory.SerialNumber + " is bad!");
                    return false;
                }
            }
            //正常内存为0
            if (j == 0)
            {
                Console.WriteLine("MemoryList does not exist!!!\n");
                return false;
            }
            Console.WriteLine();
            return true;
        }

        /// <summary>
        /// 显卡检查函数
        /// </summary>
        /// <param name="GraphicsCardList">显卡列表</param>
        /// <returns>显卡是否正常</returns>
        public bool TestGraphicsCardList(List<GraphicsCard> GraphicsCardList)
        {
            if (GraphicsCardList == null)
            {
                Console.WriteLine("GraphicsCardList does not exist!!!\n");
                return false;
            }

            int j = 0;
            foreach (GraphicsCard graphicsCard in GraphicsCardList)
            {
                if (graphicsCard == null)
                {
                    continue;
                }
                else if (graphicsCard.SerialNumber != null && graphicsCard.DeviceType == "GraphicsCard")
                {
                    Console.WriteLine(graphicsCard.SerialNumber + " is normal!");
                    j++;
                }
                else
                {
                    Console.WriteLine(graphicsCard.SerialNumber + " is bad!");
                }
            }

            if (j == 0)
            {
                Console.WriteLine("GraphicsCardList does not exist!!!\n");
                return false;
            }
            Console.WriteLine();
            return true;
        }

        /// <summary>
        /// 硬盘检查函数
        /// </summary>
        /// <param name="HardDiskList">硬盘列表</param>
        /// <returns>硬盘是否正常</returns>
        public bool TestHardDiskList(List<HardDisk> HardDiskList)
        {
            if (HardDiskList == null)
            {
                Console.WriteLine("HardDiskList does not exist!!!\n");
                return false;
            }

            int j = 0;
            foreach (HardDisk hardDisk in HardDiskList)
            {
                if (hardDisk == null)
                {
                    continue;
                }
                else if (hardDisk.SerialNumber != null && hardDisk.DeviceType == "HardDisk")
                {
                    Console.WriteLine(hardDisk.SerialNumber + " is normal!");
                    j++;
                }
                else
                {
                    Console.WriteLine(hardDisk.SerialNumber + " is bad!");
                }
            }

            if (j == 0)
            {
                Console.WriteLine("HardDiskList does not exist!!!\n");
                return false;
            }
            Console.WriteLine();
            return true;
        }

        /// <summary>
        /// USB设备检查函数
        /// </summary>
        /// <param name="UsbList">USB设备列表</param>
        /// <returns>USB设备是否正常</returns>
        public void TestUSBList(List<USB> UsbList)
        {
            foreach (USB usb in UsbList)
            {
                if (usb == null)
                {
                    continue;
                }
                else if (usb.SerialNumber != null && usb.DeviceType == "USB")
                {
                    Console.WriteLine("USB_Derive: " + usb.SerialNumber + " is normal!");
                }
                else
                {
                    Console.WriteLine("USB_Derive: " + usb.SerialNumber + " is bad!");
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 数据运算
        /// </summary>
        /// <param name="Data">数据列表</param>
        public void OperateData(List<byte> Data)
        {
        }

        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="Data">数据列表</param>
        public void ProcessData(List<byte> Data)
        {
        }
        #endregion
    }

    /// <summary>
    /// 内存接口
    /// </summary>
    public interface IMemory : IComputerSerial
    {
        #region 公有属性
        List<byte> MemorySpace { get; set; }
        #endregion
    }

    /// <summary>
    /// 内存类，继承电脑设备父类，实现内存接口
    /// </summary>
    public class Memory : ComputerSerial, IMemory
    {
        private List<byte> memorySpace;

        public List<byte> MemorySpace
        {
            get
            {
                return memorySpace;
            }
            set
            {
                memorySpace = value;
            }
        }

        /// <summary>
        /// 内存的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public Memory(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
            //一块内存包含4GB空间
            this.memorySpace = new List<byte>(4);
        }
    }

    /// <summary>
    /// 硬盘接口
    /// </summary>
    public interface IHardDisk : IComputerSerial
    {
        #region 公有属性
        List<byte> HardDiskSpace { get; set; }
        #endregion
    }

    /// <summary>
    /// 硬盘类，继承电脑设备父类，实现硬盘接口
    /// </summary>
    public class HardDisk : ComputerSerial, IHardDisk
    {
        private List<byte> hardDiskSpace;

        public List<byte> HardDiskSpace
        {
            get
            {
                return hardDiskSpace;
            }
            set
            {
                hardDiskSpace = value;
            }
        }

        /// <summary>
        /// 硬盘的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public HardDisk(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
            //一块硬盘包含256GB空间
            this.hardDiskSpace = new List<byte>(256);
        }
    }

    /// <summary>
    /// 主板接口，可以接入CPU、内存、硬盘、USB设备、显卡
    /// </summary>
    public interface IMainBoard : IComputerSerial
    {
        #region 公有属性
        /// <summary>
        /// CPU属性
        /// </summary>
        /// <value>只读</value>
        CPU MainBoardCPU { get; }
        /// <summary>
        /// 内存列表属性
        /// </summary>
        /// <value>只读</value>
        List<Memory> MainBoardMemoryList { get; }
        /// <summary>
        /// 硬盘列表属性
        /// </summary>
        /// <value>只读</value>
        List<HardDisk> MainBoardHardDiskList { get; }
        /// <summary>
        /// USB列表属性
        /// </summary>
        /// <value>可读可写</value>
        List<USB> MainBoardUSBList { get; set; }
        /// <summary>
        /// 显卡列表属性
        /// </summary>
        /// <value>只读</value>
        List<GraphicsCard> MainBoardGraphicsCardList { get; }
        #endregion

        #region 公有方法
        /// <summary>
        /// 主板供电，CPU、内存、硬盘、USB设备、显卡初始化
        /// </summary>
        void MainBoardPowerOn();

        /// <summary>
        /// 添加USB设备
        /// </summary>   
        /// <param name="usb">USB设备</param>
        void AddUSBDevice(USB usb);

        /// <summary>
        /// 移除USB设备
        /// </summary>
        /// <param name="usb">USB设备</param>
        void RemoveUSBDevice(USB usb);
        #endregion
    }

    /// <summary>
    /// 主板类，继承电脑设备父类，实现主板接口
    /// </summary>
    public class MainBoard : ComputerSerial, IMainBoard
    {
        private CPU mainBoardCPU;
        private List<Memory> mainBoardMemoryList;
        private List<HardDisk> mainBoardHardDiskList;
        private List<USB> mainBoardUSBList;
        private List<GraphicsCard> mainBoardGraphicsCardList;

        #region 公有属性
        /// <summary>
        /// CPU属性
        /// </summary>
        /// <value>只读</value>
        public CPU MainBoardCPU
        {
            get
            {
                return mainBoardCPU;
            }
        }

        /// <summary>
        /// 内存列表属性
        /// </summary>
        /// <value>只读</value>
        public List<Memory> MainBoardMemoryList
        {
            get
            {
                return mainBoardMemoryList;
            }
        }

        /// <summary>
        /// 硬盘列表属性
        /// </summary>
        /// <value>只读</value>
        public List<HardDisk> MainBoardHardDiskList
        {
            get
            {
                return mainBoardHardDiskList;
            }
        }

        /// <summary>
        /// USB列表属性
        /// </summary>
        /// <value>可读可写</value>
        public List<USB> MainBoardUSBList
        {
            get
            {
                return mainBoardUSBList;
            }
            set
            {
                mainBoardUSBList = value;
            }
        }

        /// <summary>
        /// 显卡列表属性
        /// </summary>
        /// <value>只读</value>
        public List<GraphicsCard> MainBoardGraphicsCardList
        {
            get
            {
                return mainBoardGraphicsCardList;
            }
        }
        #endregion

        /// <summary>
        /// 主板的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public MainBoard(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;

            this.mainBoardMemoryList = new List<Memory>(4);
            this.mainBoardHardDiskList = new List<HardDisk>(2);
            this.mainBoardUSBList = new List<USB>(6);
            this.mainBoardGraphicsCardList = new List<GraphicsCard>(2);
        }

        #region 公有方法
        /// <summary>
        /// 主板供电，CPU、内存、硬盘、USB设备、显卡初始化
        /// </summary>
        public void MainBoardPowerOn()
        {
            Memory mainBoardMemory1 = new Memory("Memory1", "Memory");
            Memory mainBoardMemory2 = new Memory("Memory2", "Memory");

            HardDisk hardDisk1 = new HardDisk("HardDisk1", "HardDisk");
            HardDisk hardDisk2 = new HardDisk("HardDisk2", "HardDisk");
            HardDisk hardDisk3 = new HardDisk("HardDisk3", "HardDisk");

            USB mainBoardMouse = new USB("Mouse1", "USB");
            USB mainBoardKeyBoard = new USB("KeyBoard1", "USB");

            GraphicsCard mainBoardGraphicsCard1 = new GraphicsCard("GraphicsCard1", "GraphicsCard");
            GraphicsCard mainBoardGraphicsCard2 = new GraphicsCard("GraphicsCard2", "GraphicsCard");

            //CPU、内存、硬盘、USB设备、显卡初始化
            this.mainBoardCPU = new CPU("CPU1", "CPU");

            this.mainBoardMemoryList.Add(mainBoardMemory1);
            this.mainBoardMemoryList.Add(mainBoardMemory2);

            this.mainBoardHardDiskList.Add(hardDisk1);
            this.mainBoardHardDiskList.Add(hardDisk2);
            this.mainBoardHardDiskList.Add(hardDisk3);

            this.mainBoardUSBList.Add(mainBoardMouse);
            this.mainBoardUSBList.Add(mainBoardKeyBoard);

            this.mainBoardGraphicsCardList.Add(mainBoardGraphicsCard1);
            this.mainBoardGraphicsCardList.Add(mainBoardGraphicsCard2);
        }

        /// <summary>
        /// 添加USB设备
        /// </summary>
        /// <param name="usb">USB设备</param>
        public void AddUSBDevice(USB usb)
        {
            if (this.MainBoardUSBList.Count < this.MainBoardUSBList.Capacity)
                this.MainBoardUSBList.Add(usb);
        }

        /// <summary>
        /// 移除USB设备
        /// </summary>
        /// <param name="usb">USB设备</param>
        public void RemoveUSBDevice(USB usb)
        {
            this.MainBoardUSBList.Remove(usb);
        }
        #endregion
    }

    /// <summary>
    /// 主机接口
    /// </summary>
    public interface IHost : IComputerSerial
    {
        #region 公有属性
        /// <summary>
        /// 电源属性
        /// </summary>
        /// <value>只读</value>
        IPowerSupply HostPowerSupply { get; }
        /// <summary>
        /// 主板属性
        /// </summary>
        /// <value>只读</value>
        IMainBoard HostMainBoard { get; }
        #endregion

        #region 公有方法
        /// <summary>
        /// 向其它设备供电
        /// </summary>
        void HostPowerOn();
        #endregion
    }

    /// <summary>
    /// 主机类，继承电脑设备父类，实现主机接口
    /// </summary>
    public class Host : ComputerSerial, IHost
    {
        private PowerSupply hostPowerSupply;
        private MainBoard hostMainBoard;

        #region 公有属性
        /// <summary>
        /// 电源属性
        /// </summary>
        /// <value>只读</value>
        public IPowerSupply HostPowerSupply
        {
            get
            {
                return hostPowerSupply;
            }
        }

        /// <summary>
        /// 主板属性
        /// </summary>
        /// <value>只读</value>
        public IMainBoard HostMainBoard
        {
            get
            {
                return hostMainBoard;
            }
        }
        #endregion

        /// <summary>
        /// 主机的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public Host(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;

            //电源、主板初始化
            hostPowerSupply = new PowerSupply("PowerSupply1", "PowerSupply", 5, 220);
            hostMainBoard = new MainBoard("MainBoard1", "MainBoard");
        }

        #region 公有方法
        /// <summary>
        /// 向其它设备供电
        /// </summary>
        public void HostPowerOn()
        {
            //主板向其它设备供电
            hostMainBoard.MainBoardPowerOn();
        }
        #endregion
    }

    /// <summary>
    /// 显卡接口
    /// </summary>
    public interface IGraphicsCard : IComputerSerial
    {
        #region 公有方法
        /// <summary>
        /// 将传递过来的序列号经过信号转换后，再传递出去
        /// </summary>
        /// <param name="serialNumber">设备序列号</param>
        /// <returns>设备序列号</returns>
        string GetSerialNumber(string serialNumber);

        /// <summary>
        /// 将传递过来的设备类型经过信号转换后，再传递出去
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <returns>设备类型</returns>
        string GetDeviceType(string deviceType);

        /// <summary>
        /// 将传递过来的图像信息经过信号转换后，再传递出去
        /// </summary>
        /// <param name="imageSignal">图像信号</param>
        /// <returns>图像信号</returns>
        string GetImageSignal(string imageSignal);
        #endregion
    }

    /// <summary>
    /// 显卡类，继承电脑设备父类，实现显卡接口
    /// </summary>
    public class GraphicsCard : ComputerSerial, IGraphicsCard
    {
        #region 公有方法
        /// <summary>
        /// 显卡的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public GraphicsCard(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
        }

        /// <summary>
        /// 将传递过来的序列号经过信号转换后，再传递给显示器
        /// </summary>
        /// <param name="serialNumber">设备序列号</param>
        /// <returns>设备序列号</returns>
        public string GetSerialNumber(string serialNumber)
        {
            return serialNumber;
        }

        /// <summary>
        /// 将传递过来的设备类型经过信号转换后，再传递给显示器
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <returns>设备类型</returns>
        public string GetDeviceType(string deviceType)
        {
            return deviceType;
        }

        /// <summary>
        /// 将传递过来的图像信息经过信号转换后，再传递出去
        /// </summary>
        /// <param name="imageSignal">图像信号</param>
        /// <returns>图像信号</returns>
        public string GetImageSignal(string imageSignal)
        {
            return imageSignal;
        }
        #endregion
    }

    /// <summary>
    /// 显示器接口
    /// </summary>
    public interface IMonitor : IComputerSerial
    {
        #region 公有方法
        /// <summary>
        /// 显示设备信息函数
        /// </summary>
        /// <param name="message">需要显示的设备信息</param>
        void ShowMessage(string message);

        /// <summary>
        /// 显示图像
        /// </summary>
        /// <param name="imageSignal">图像信号</param>
        void ShowImage(string imageSignal);
        #endregion
    }

    /// <summary>
    /// 显示器类，继承电脑设备父类，实现显示器接口
    /// </summary>
    public class Monitor : ComputerSerial, IMonitor
    {
        /// <summary>
        /// 显示器的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public Monitor(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
        }

        #region 公有方法
        /// <summary>
        /// 显示设备信息函数
        /// </summary>
        /// <param name="message">需要显示的设备信息</param>
        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// 显示图像
        /// </summary>
        /// <param name="imageSignal">图像信号</param>
        public void ShowImage(string imageSignal)
        {
            Console.WriteLine(imageSignal);
        }
        #endregion
    }

    /// <summary>
    /// USB接口
    /// </summary>
    public interface IUSB : IComputerSerial
    {
    }

    /// <summary>
    /// USB类，继承电脑设备父类，实现USB接口
    /// </summary>
    public class USB : ComputerSerial, IUSB
    {
        /// <summary>
        /// USB的带参构造函数
        /// </summary>
        /// <param name="serialNumber">序列号</param>
        /// <param name="deviceType">设备类型</param>
        public USB(string serialNumber, string deviceType)
        {
            this.serialNumber = serialNumber;
            this.deviceType = deviceType;
        }
    }
}