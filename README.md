# Lucas Square Pyramid Finder (Async)

* An interesting problem in arithmetic with deep implications to elliptic curve theory is the problem of ﬁnding perfect squares that are sums of consecutive
squares. An interesting example is Lucas' Square Pyramid: 1^2 + 2^2 + 3^2 + .... + 24^2 = 70^2
* The goal of this project is to use F# Akka.NET actor model to build an efficient solution to this problem with concurrency, that runs well on multi-core machines.

## To execute the application:

For example, for the input N = 1000000 and k = 4, run the command as: dotnet fsi –langversion:preview proj1.fsx 1000000 4
