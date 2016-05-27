# Securing TCP Qbservable Provider Services

### Kinds of Security:

- Semantic
	- Limit API surface via whitelisting
- Resources (memory, CPU, HDD, Network)
	- API
		- Limit API surface via CAS
		- Limit API surface via whitelisting
	- Algorithmic
		- Semantic analysis of expression trees; e.g., limit operators related to buffering, instantiation and any 
			other kinds of potentially unbounded memory usage.

### Guidelines: 

- Prefer whitelisting over blacklisting.
- Where "Allow" and "Disallow" are specified below, they only represent default behavior and should be configurable by hosts.

### Secure model: 

- Allow primitives, Guid, Uri, Enum, TimeSpan, DateTimeOffset and anonymous types
- Allow arrays and all types in the System.Collections.* namespaces (only when created by the service)
- Allow Qbservable, Observable, Enumerable and Queryable operators.
- Allow Observers (only when created by the service; required for callbacks; e.g., see chat example).
- Disallow use of System.Type entirely (can't even reference it as a return value).
- Disallow all types and members in the System.Reflection namespace.
- Disallow all types and members in the System.Security namespace.

The following requirements apply to secure expression tree visitors for evaluating expression nodes, including all 
operators defined by Qbservable, Observable, Enumerable and Queryable, where appropriate.

- Whitelist operators that are considered completely secure.
	- Disallow overloads that accept schedulers (or simply disallow references to singleton schedulers and scheduler constructors)
	- Disallow blocking operators.
	- Disallow operators that use buffering (even internal) that could be abused or is potentially unbounded
		- e.g., Distinct, GroupBy, Join, Buffer, Window
		- Disallow aggregates with custom selectors, and also those with potentially unbounded wrappers like ToList.
- Whitelist types that can be constructed
	- e.g., primitives, Guid, Uri, Enum, TimeSpan, DateTimeOffset and anonymous types
	- Disallow arrays and types in the System.Collections.* namespaces
- Disallow all block nodes.
- Disallow all void methods.
- Disallow field and property assignments (except anonymous types).

Place configurable caps on the use of some kinds of types, operators and parameters: 

- Limit the size of strings
- Limit the number of strings
- Allow only 1 use of Observable.Timer or Observable.Interval per query
	- Duration must be >= 30 seconds and <= 12 hours
- Observable.Range length must be <= 10
- Limit the number of operators that are permitted.
- Limit the size of the serialized expression tree bytes that is permitted for deserialization.
- Limit the size of all data payloads, including duplex.
	- Provide distinct settings between initial input (e.g., expression) vs. incoming duplex (e.g., OnNext and member values).
- Throttle duplex observables by applying a minimum duration of 30 seconds.
- Throttle service output by applying a minimum duration of 30 seconds.
