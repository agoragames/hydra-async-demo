using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketSharp
{
    public delegate void Action();
    public delegate void Action<T1, T2>(T1 one, T2 two);
}
