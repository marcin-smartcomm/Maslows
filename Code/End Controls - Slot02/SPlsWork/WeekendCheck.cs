using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using Crestron;
using Crestron.Logos.SplusLibrary;
using Crestron.Logos.SplusObjects;
using Crestron.SimplSharp;

namespace UserModule_WEEKENDCHECK
{
    public class UserModuleClass_WEEKENDCHECK : SplusObject
    {
        static CCriticalSection g_criticalSection = new CCriticalSection();
        
        
        Crestron.Logos.SplusObjects.DigitalInput CHECK;
        Crestron.Logos.SplusObjects.DigitalOutput WEEKDAY;
        Crestron.Logos.SplusObjects.DigitalOutput WEEKEND;
        object CHECK_OnPush_0 ( Object __EventInfo__ )
        
            { 
            Crestron.Logos.SplusObjects.SignalEventArgs __SignalEventArg__ = (Crestron.Logos.SplusObjects.SignalEventArgs)__EventInfo__;
            try
            {
                SplusExecutionContext __context__ = SplusThreadStartCode(__SignalEventArg__);
                
                __context__.SourceCodeLine = 11;
                if ( Functions.TestForTrue  ( ( Functions.BoolToInt ( (Functions.TestForTrue ( Functions.BoolToInt (Functions.Day() == "Saturday") ) || Functions.TestForTrue ( Functions.BoolToInt (Functions.Day() == "Sunday") )) ))  ) ) 
                    { 
                    __context__.SourceCodeLine = 13;
                    WEEKDAY  .Value = (ushort) ( 0 ) ; 
                    __context__.SourceCodeLine = 14;
                    WEEKEND  .Value = (ushort) ( 1 ) ; 
                    } 
                
                else 
                    { 
                    __context__.SourceCodeLine = 18;
                    WEEKDAY  .Value = (ushort) ( 1 ) ; 
                    __context__.SourceCodeLine = 19;
                    WEEKEND  .Value = (ushort) ( 0 ) ; 
                    } 
                
                
                
            }
            catch(Exception e) { ObjectCatchHandler(e); }
            finally { ObjectFinallyHandler( __SignalEventArg__ ); }
            return this;
            
        }
        
    
    public override void LogosSplusInitialize()
    {
        _SplusNVRAM = new SplusNVRAM( this );
        
        CHECK = new Crestron.Logos.SplusObjects.DigitalInput( CHECK__DigitalInput__, this );
        m_DigitalInputList.Add( CHECK__DigitalInput__, CHECK );
        
        WEEKDAY = new Crestron.Logos.SplusObjects.DigitalOutput( WEEKDAY__DigitalOutput__, this );
        m_DigitalOutputList.Add( WEEKDAY__DigitalOutput__, WEEKDAY );
        
        WEEKEND = new Crestron.Logos.SplusObjects.DigitalOutput( WEEKEND__DigitalOutput__, this );
        m_DigitalOutputList.Add( WEEKEND__DigitalOutput__, WEEKEND );
        
        
        CHECK.OnDigitalPush.Add( new InputChangeHandlerWrapper( CHECK_OnPush_0, false ) );
        
        _SplusNVRAM.PopulateCustomAttributeList( true );
        
        NVRAM = _SplusNVRAM;
        
    }
    
    public override void LogosSimplSharpInitialize()
    {
        
        
    }
    
    public UserModuleClass_WEEKENDCHECK ( string InstanceName, string ReferenceID, Crestron.Logos.SplusObjects.CrestronStringEncoding nEncodingType ) : base( InstanceName, ReferenceID, nEncodingType ) {}
    
    
    
    
    const uint CHECK__DigitalInput__ = 0;
    const uint WEEKDAY__DigitalOutput__ = 0;
    const uint WEEKEND__DigitalOutput__ = 1;
    
    [SplusStructAttribute(-1, true, false)]
    public class SplusNVRAM : SplusStructureBase
    {
    
        public SplusNVRAM( SplusObject __caller__ ) : base( __caller__ ) {}
        
        
    }
    
    SplusNVRAM _SplusNVRAM = null;
    
    public class __CEvent__ : CEvent
    {
        public __CEvent__() {}
        public void Close() { base.Close(); }
        public int Reset() { return base.Reset() ? 1 : 0; }
        public int Set() { return base.Set() ? 1 : 0; }
        public int Wait( int timeOutInMs ) { return base.Wait( timeOutInMs ) ? 1 : 0; }
    }
    public class __CMutex__ : CMutex
    {
        public __CMutex__() {}
        public void Close() { base.Close(); }
        public void ReleaseMutex() { base.ReleaseMutex(); }
        public int WaitForMutex() { return base.WaitForMutex() ? 1 : 0; }
    }
     public int IsNull( object obj ){ return (obj == null) ? 1 : 0; }
}


}
