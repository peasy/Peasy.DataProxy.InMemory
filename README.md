![peasy](https://www.dropbox.com/s/2yajr2x9yevvzbm/peasy3.png?dl=0&raw=1)

### Peasy.DataProxy.InMemory

Peasy.DataProxy.InMemory provides the [InMemoryDataProxyBase](https://github.com/ahanusa/Peasy.DataProxy.InMemory/blob/master/Peasy.DataProxy.InMemory/InMemoryDataProxyBase.cs) class.  InMemoryDataProxyBase is an abstract class that implements [IDataProxy](https://github.com/ahanusa/Peasy.NET/wiki/Data-Proxy), and can be used to provide an in-memory implementation of your data layer.

Creating an in-memory implementation of your data layer can help developing state-based unit tests as well as serving as a fake repository for rapid development of your [service classes](https://github.com/ahanusa/Peasy.NET/wiki/ServiceBase).

InMemoryDataProxy was designed to be simple to use and is thread-safe.

