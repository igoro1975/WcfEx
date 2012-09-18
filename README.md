WcfEx
=====
Yet another lightweight WCF extensions library for .NET 4.0+

Features:
---------
* Generic client channel wrappers for simplex and duplex contracts
* A generic WCF host executable for DLL-based services, optionally 
installed as a Windows service
* A UDP transport layer, for input/output and request/reply channels
* An in-process transport layer and automatic client host, for 
configuration-based hosting within the client process
* GZip/Deflate message encoders, for compressing buffered or streamed messages
* Base classes for the WCF bindings, channels, listeners, factories, and 
behaviors with default sync or async methods
* Synchronous and asynchronous `IAsyncResult` implementation utilities, for 
simplified APM in WCF

###Client Channel Wrappers###
    // default configuration-based endpoint
    using (var client = new WcfEx.Client<IService>()
      client.Server.Execute("test");

    // named configuration-based endpoint
    using (var client = new WcfEx.Client<IService>("MyEndpoint")
      client.Server.Execute("test");

    // explicitly configured endpoint
    using (var client = new WcfEx.Client<IService>(new NetNamedPipeBinding(), "net.pipe://localhost/Test"))
      client.Server.Execute("test");

###WCF Host Application###
This project includes a generic Windows application (`WcfExHost.exe`) that 
can be used to host WCF DLL services created via the **WCF Service Application** 
project template. It is a lightweight alternative to IIS/WAS hosting.

Simply deploy `WcfExHost.exe` in the same directory as your service, and 
start the executable. It will load any `.dll.config` files in that 
directory and create a WCF ServiceHost instance for each configured service.

You may also register the host to run as a Windows service, using
`InstallUtil`. The name of the Windows service is customizable via install
parameters.

###UDP Transport Layer###
Of course, no WCF library would not be complete without the requisite UDP 
transport binding. However, this version goes a bit further than the MSDN 
samples, including full IPV6 support and a fully re-entrant request channel 
that optimizes one-way requests by not sending reply messages. The UDP 
transport layer is also fully compatible with the composite duplex and 
reliable transport bindings.

###Automatic Hosting and the In-Process Transport Layer###
Occasionally, it is necessary to host a WCF service within the client 
process, when implementing a mocked version of the service for testing, for
example. However, using the `ServiceHost` directly within the client 
application couples the client to that implementation. Ideally, embedding 
the service host or mock in the client could be done tranparently via 
configuration.

WcfEx provides this with the `ClientHostBehavior` extension. When this
behavior is added to a client endpoint, it automatically starts a host for 
the configured service when the client channel is first created.

    <system.serviceModel>
       <extensions>
          <behaviorExtensions>
             <add name="clientHost" type="WcfEx.ClientHostBehavior,WcfEx"/>
          </behaviorExtensions>
       </extensions>
       <services>
          <service name="MyMockService">
             <endpoint contract="IMyService">...</endpoint>
          </service>
       </services>
       <client>
          <endpoint contract="IMyService">...</endpoint>
       </client>
       <behaviors>
          <endpointBehaviors>
             <behavior>
                <clientHost/>
             </behavior>
          </endpointBehaviors>
       </behaviors>
    </system.serviceModel>

In addition, the library includes the `<inProcTransport>` binding that
bypasses the system networking infrastructure and WCF encoding stack,
transferring messages directly between the client and service within the 
same process. This is useful both for testing the service and for planning 
deployment flexibility without committing to a remote service installation. 
Because it bypasses the encoding layer, the in-process transport also 
performs significantly (10% in tests) better than even the named pipe transport.

###Compression Encodings###
The WcfEx library includes GZip/deflate message encoders for compressing 
WCF messages before sending them to transport. The compression encoders work
with both buffered and streamed messages.

###WCF Base Classes###
Frequently when using WCF, it is necessary to extend the framework with
custom functionality using the extension interfaces. However, implementing 
custom channel stacks and behaviors is tedious, particularly the Begin/End 
APM methods.

WcfEx includes an extensive library of base classes to ease the
implementation and registration of these components. These base classes are 
used heavily within WcfEx to simplify its extension classes. In most cases, 
the user can simply implement either the sync or async side of an interface 
and rely on the base class's implementation of the other.

###APM Utilities###
Implementing the IAsyncResult interface is a straightforward, but tedious 
thing to do. However, implementing it correctly and efficiently in a 
concurrent environment with exceptions is devilishly tricky.

The WcfEx SyncResult and AsyncResult classes provide a concurrent and 
exception-correct implementation of IAsyncResult for synchronous and 
asynchronous operations, respectively. These classes also lazily allocate 
the AsyncWaitHandle property, avoiding an expensive event handle allocation
when it is not used.

For synchronous operations:

    public String Trim (String str)
    {
       return str.Trim();
    }
    public IAsyncResult BeginTrim (String str, AsyncCallback callback, Object state)
    {
       var trimmed = Trim(str);
       return new SyncResult(callback, state, trimmed);
    }
    public String EndTrim (IAsyncResult result)
    {
       return ((SyncResult)result).GetResult<String>();
    }

For asynchronous operations:

    public String Trim (String str, TimeSpan timeout)
    {
       return EndTrim(BeginTrim(str, timeout, null, null));
    }
    public IAsyncResult BeginTrim (String str, TimeSpan timeout, AsyncCallback callback, Object state)
    {
       var result = new AsyncResult(callback, state, timeout);
       ThreadPool.QueueUserWorkItem(o => result.Complete(str.Trim()), null);
       return result;
    }
    public String EndTrim (IAsyncResult result)
    {
       return ((AsyncResult)result).GetResult<String>();
    }
