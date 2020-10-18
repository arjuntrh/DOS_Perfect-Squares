#time "on"
#r "nuget: Akka.FSharp" 

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

let system = ActorSystem.Create("FSharp")

type ChildActorMessage = ProcessActorRange of int * int * int * int

type ParentActorMessage = 
    |ProcessInput of int * int
    |ProcessChildInput of int

let mutable mainRef = Unchecked.defaultof<_>
let mutable childActorCount = 0


(* Child Actor computes the squares of consecutive numbers and checks the sum of k squares for any possible perfect square
   If the sum if perfect square, it prints the first number in the k sequence *)
let childActor (mailbox: Actor<_>) = 
    let rec childActorLoop () = actor {

        let! ProcessActorRange(startRange, endRange, k, n) = mailbox.Receive ()
        let childActorSender = mailbox.Sender()

        let nums = new List<int>()
        
        for i = startRange to endRange do
            let mutable sum = 0UL

            if (i + k - 1 <= n) then
                for j = 0 to k-1 do
                    let num = uint64 (i+j) 
                    sum <- sum + (num*num)

                let sq = sqrt (double sum)
                if sq = floor sq then
                    nums.Add(i)

        for num in nums do
            printfn "%i" num

        childActorSender <! ProcessChildInput(1)
        return! childActorLoop ()
    }
    childActorLoop ()


(* Parent Actor handles the calculation of number of child actors to be created to accomplish the task
   It creates 'numActors' of child actors and call them with their range of input numbers i.e. actorRange *)
let parentActor (mailbox: Actor<_>) =

    let rec parentActorLoop () = actor {
        
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()

        let mutable numActors = 1000
        match message with
        | ProcessInput(n,k) ->
                    mainRef <- sender

                    let mutable actorRange = n/numActors

                    if (actorRange = 0) then
                        actorRange <- n;

                    let mutable start = 1

                    let childActors = 
                        [1 .. numActors]
                        |> List.map(fun id -> spawn system (string(id)) childActor)

                    for server in childActors do
                        let mutable endRange = start + actorRange - 1
                        endRange <- min n endRange
                        server <! ProcessActorRange(start, endRange, k, n)
                        start <- start + actorRange

                        
        | ProcessChildInput(count) ->
            childActorCount <- childActorCount + 1
            if childActorCount = numActors then
                mainRef <! "All Actors completed the Task"

        return! parentActorLoop()
    }

    parentActorLoop ()



(* Main Function to accept the command line arguments - N and K
   ParentActor is called from Main to calculate the number of actors need to be created
*)
let main() =
    try
        let n = fsi.CommandLineArgs.[1] |> int
        let k = fsi.CommandLineArgs.[2] |> int

        let parentActorRef = spawn system "parentActor" parentActor
        let parentTask = (parentActorRef <? ProcessInput(n,k))
        let response = Async.RunSynchronously(parentTask)
        printf ""

    with :? TimeoutException ->
        printfn "Timeout!"

main()
system.Terminate()




