//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: GameProtocol.proto
// Note: requires additional types generated from: EpochProtocol.proto
namespace GameProtocol
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPParam")]
  public partial class FSPParam : global::ProtoBuf.IExtensible
  {
    public FSPParam() {}
    
    private string _host;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"host", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string host
    {
      get { return _host; }
      set { _host = value; }
    }
    private int _port;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"port", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int port
    {
      get { return _port; }
      set { _port = value; }
    }
    private uint _sid;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"sid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint sid
    {
      get { return _sid; }
      set { _sid = value; }
    }
    private int _serverFrameInterval = (int)66;
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"serverFrameInterval", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)66)]
    public int serverFrameInterval
    {
      get { return _serverFrameInterval; }
      set { _serverFrameInterval = value; }
    }
    private int _serverTimeout = (int)15000;
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"serverTimeout", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)15000)]
    public int serverTimeout
    {
      get { return _serverTimeout; }
      set { _serverTimeout = value; }
    }
    private int _clientFrameRateMultiple = (int)2;
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"clientFrameRateMultiple", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)2)]
    public int clientFrameRateMultiple
    {
      get { return _clientFrameRateMultiple; }
      set { _clientFrameRateMultiple = value; }
    }
    private bool _enableSpeedUp = (bool)true;
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"enableSpeedUp", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue((bool)true)]
    public bool enableSpeedUp
    {
      get { return _enableSpeedUp; }
      set { _enableSpeedUp = value; }
    }
    private int _defaultSpeed = (int)1;
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"defaultSpeed", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)1)]
    public int defaultSpeed
    {
      get { return _defaultSpeed; }
      set { _defaultSpeed = value; }
    }
    private int _frameBufferSize = default(int);
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"frameBufferSize", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int frameBufferSize
    {
      get { return _frameBufferSize; }
      set { _frameBufferSize = value; }
    }
    private bool _enableAutoBuffer = (bool)true;
    [global::ProtoBuf.ProtoMember(10, IsRequired = false, Name=@"enableAutoBuffer", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue((bool)true)]
    public bool enableAutoBuffer
    {
      get { return _enableAutoBuffer; }
      set { _enableAutoBuffer = value; }
    }
    private int _maxFrameId = (int)1800;
    [global::ProtoBuf.ProtoMember(11, IsRequired = false, Name=@"maxFrameId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue((int)1800)]
    public int maxFrameId
    {
      get { return _maxFrameId; }
      set { _maxFrameId = value; }
    }
    private int _authId = default(int);
    [global::ProtoBuf.ProtoMember(12, IsRequired = false, Name=@"authId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int authId
    {
      get { return _authId; }
      set { _authId = value; }
    }
    private bool _useLocal = default(bool);
    [global::ProtoBuf.ProtoMember(13, IsRequired = false, Name=@"useLocal", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(default(bool))]
    public bool useLocal
    {
      get { return _useLocal; }
      set { _useLocal = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPVKey")]
  public partial class FSPVKey : global::ProtoBuf.IExtensible
  {
    public FSPVKey() {}
    
    private int _vkey;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"vkey", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int vkey
    {
      get { return _vkey; }
      set { _vkey = value; }
    }
    private readonly global::System.Collections.Generic.List<int> _args = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(2, Name=@"args", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> args
    {
      get { return _args; }
    }
  
    private uint _playerIdOrClientFrameId;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"playerIdOrClientFrameId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint playerIdOrClientFrameId
    {
      get { return _playerIdOrClientFrameId; }
      set { _playerIdOrClientFrameId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPData_CS")]
  public partial class FSPData_CS : global::ProtoBuf.IExtensible
  {
    public FSPData_CS() {}
    
    private uint _sid;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"sid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint sid
    {
      get { return _sid; }
      set { _sid = value; }
    }
    private readonly global::System.Collections.Generic.List<GameProtocol.FSPVKey> _vkeys = new global::System.Collections.Generic.List<GameProtocol.FSPVKey>();
    [global::ProtoBuf.ProtoMember(2, Name=@"vkeys", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<GameProtocol.FSPVKey> vkeys
    {
      get { return _vkeys; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPFrame")]
  public partial class FSPFrame : global::ProtoBuf.IExtensible
  {
    public FSPFrame() {}
    
    private int _frameId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"frameId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int frameId
    {
      get { return _frameId; }
      set { _frameId = value; }
    }
    private readonly global::System.Collections.Generic.List<GameProtocol.FSPVKey> _vkeys = new global::System.Collections.Generic.List<GameProtocol.FSPVKey>();
    [global::ProtoBuf.ProtoMember(2, Name=@"vkeys", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<GameProtocol.FSPVKey> vkeys
    {
      get { return _vkeys; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPData_SC")]
  public partial class FSPData_SC : global::ProtoBuf.IExtensible
  {
    public FSPData_SC() {}
    
    private readonly global::System.Collections.Generic.List<GameProtocol.FSPFrame> _frames = new global::System.Collections.Generic.List<GameProtocol.FSPFrame>();
    [global::ProtoBuf.ProtoMember(1, Name=@"frames", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<GameProtocol.FSPFrame> frames
    {
      get { return _frames; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPPlayerData")]
  public partial class FSPPlayerData : global::ProtoBuf.IExtensible
  {
    public FSPPlayerData() {}
    
    private uint _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint id
    {
      get { return _id; }
      set { _id = value; }
    }
    private string _name = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string name
    {
      get { return _name; }
      set { _name = value; }
    }
    private uint _userId = default(uint);
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"userId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(uint))]
    public uint userId
    {
      get { return _userId; }
      set { _userId = value; }
    }
    private uint _sid = default(uint);
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"sid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(uint))]
    public uint sid
    {
      get { return _sid; }
      set { _sid = value; }
    }
    private bool _isReady = default(bool);
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"isReady", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(default(bool))]
    public bool isReady
    {
      get { return _isReady; }
      set { _isReady = value; }
    }
    private uint _teamId = default(uint);
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"teamId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(uint))]
    public uint teamId
    {
      get { return _teamId; }
      set { _teamId = value; }
    }
    private byte[] _customPlayerData = null;
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"customPlayerData", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public byte[] customPlayerData
    {
      get { return _customPlayerData; }
      set { _customPlayerData = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPGameStartParam")]
  public partial class FSPGameStartParam : global::ProtoBuf.IExtensible
  {
    public FSPGameStartParam() {}
    
    private GameProtocol.FSPParam _fspParam;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"fspParam", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public GameProtocol.FSPParam fspParam
    {
      get { return _fspParam; }
      set { _fspParam = value; }
    }
    private readonly global::System.Collections.Generic.List<GameProtocol.FSPPlayerData> _players = new global::System.Collections.Generic.List<GameProtocol.FSPPlayerData>();
    [global::ProtoBuf.ProtoMember(2, Name=@"players", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<GameProtocol.FSPPlayerData> players
    {
      get { return _players; }
    }
  
    private byte[] _customGameParam = null;
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"customGameParam", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public byte[] customGameParam
    {
      get { return _customGameParam; }
      set { _customGameParam = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FSPRoomData")]
  public partial class FSPRoomData : global::ProtoBuf.IExtensible
  {
    public FSPRoomData() {}
    
    private uint _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint id
    {
      get { return _id; }
      set { _id = value; }
    }
    private readonly global::System.Collections.Generic.List<GameProtocol.FSPPlayerData> _players = new global::System.Collections.Generic.List<GameProtocol.FSPPlayerData>();
    [global::ProtoBuf.ProtoMember(2, Name=@"players", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<GameProtocol.FSPPlayerData> players
    {
      get { return _players; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"GameMode")]
    public enum GameMode
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"TimelimitPVE", Value=1)]
      TimelimitPVE = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"EndlessPVE", Value=2)]
      EndlessPVE = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"TimelimitPVP", Value=3)]
      TimelimitPVP = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"EndlessPVP", Value=4)]
      EndlessPVP = 4
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"GameVKey")]
    public enum GameVKey
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"MoveX", Value=1)]
      MoveX = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"MoveY", Value=2)]
      MoveY = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"SpeedUp", Value=3)]
      SpeedUp = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"CreatePlayer", Value=4)]
      CreatePlayer = 4
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"FSPVKeyBase")]
    public enum FSPVKeyBase
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"GAME_BEGIN", Value=100)]
      GAME_BEGIN = 100,
            
      [global::ProtoBuf.ProtoEnum(Name=@"ROUND_BEGIN", Value=101)]
      ROUND_BEGIN = 101,
            
      [global::ProtoBuf.ProtoEnum(Name=@"LOAD_START", Value=102)]
      LOAD_START = 102,
            
      [global::ProtoBuf.ProtoEnum(Name=@"LOAD_PROGRESS", Value=103)]
      LOAD_PROGRESS = 103,
            
      [global::ProtoBuf.ProtoEnum(Name=@"CONTROL_START", Value=104)]
      CONTROL_START = 104,
            
      [global::ProtoBuf.ProtoEnum(Name=@"GAME_EXIT", Value=105)]
      GAME_EXIT = 105,
            
      [global::ProtoBuf.ProtoEnum(Name=@"ROUND_END", Value=106)]
      ROUND_END = 106,
            
      [global::ProtoBuf.ProtoEnum(Name=@"GAME_END", Value=107)]
      GAME_END = 107,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AUTH", Value=108)]
      AUTH = 108,
            
      [global::ProtoBuf.ProtoEnum(Name=@"PING", Value=109)]
      PING = 109
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"FSPGameState")]
    public enum FSPGameState
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"None", Value=0)]
      None = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"Create", Value=1)]
      Create = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"GameBegin", Value=2)]
      GameBegin = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"RoundBegin", Value=3)]
      RoundBegin = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"ControlStart", Value=4)]
      ControlStart = 4,
            
      [global::ProtoBuf.ProtoEnum(Name=@"RoundEnd", Value=5)]
      RoundEnd = 5,
            
      [global::ProtoBuf.ProtoEnum(Name=@"GameEnd", Value=6)]
      GameEnd = 6
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"FSPGameEndReason")]
    public enum FSPGameEndReason
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"Normal", Value=0)]
      Normal = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AllOtherExit", Value=1)]
      AllOtherExit = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AllOtherLost", Value=2)]
      AllOtherLost = 2
    }
  
}