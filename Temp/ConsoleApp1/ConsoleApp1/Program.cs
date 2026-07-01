// this should be easy by now
using System.Diagnostics;
using System.Runtime.InteropServices.Swift;

static int ClassicFib(int n)
  => n switch { < 2 => n, _ => ClassicFib(n - 1) + ClassicFib(n - 2) };

// this takes a storage + function and creates a new function
static Func<int, int> Memoize(Dictionary<int, int> memo, Func<int, int> f)
{
    memo[0] = 0;
    memo[1] = 1;
    memo[2] = 1;
    Func<int, int> self = null;
    self = n =>
        {
            if (n < 2) return n;
            if (memo.TryGetValue(n, out var m)) return m;
            if (!memo.TryGetValue(n - 2, out var y)) y = self(n - 2);
            if (!memo.TryGetValue(n - 1, out var x)) x = self(n - 1);
            memo[n] = x + y;
            return x + y;
        };
    return self;
}

static Func<int, int> MemoizeMartin(Dictionary<int, int> memo, Func<Func<int, int>, int, int> f)
{
    Func<int, int> self = null!;
    self = n => memo.TryGetValue(n, out var m) ? m : memo[n] = f(self, n);
    return self;
}



// create store and build the new function
Dictionary<int, int> store = [];
Dictionary<int, int> storeMartin = [];
var memoizedFib = Memoize(store, ClassicFib);
var memoizedFibMartin = MemoizeMartin(storeMartin, (f, n) => n switch { < 2 => n, _ => f(n - 1) + f(n - 2) });

const int testN = 200_000_000;
const int iterations = 1;

//Console.WriteLine(Benchmark(ClassicFib, iterations, testN));
Console.WriteLine(Benchmark(memoizedFib, iterations, testN));
Console.WriteLine(Benchmark(memoizedFibMartin, iterations, testN));

List<int> myList = store.Values.ToList();
List<int> martinList = storeMartin.Values.ToList();

Debug.Assert(myList.Count == martinList.Count);
for (int i = 0; i < myList.Count; i++)
{
    Debug.Assert(myList[i] == martinList[i], $"my num is {myList[i]} and martis is {martinList[i]}");
}

TimeSpan Benchmark(Func<int, int> f, int iterations, int n)
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
        for(int j =  0; j < n; j++)
            _ = f(j);
    sw.Stop();
    return sw.Elapsed;
}