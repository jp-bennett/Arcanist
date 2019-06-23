using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArcaneTide.Utils {
    static class OtherUtils {
        public static string GetMd5(string str) {
            var md5 = MD5.Create();
            var bytValue = Encoding.UTF8.GetBytes(str);
            var bytHash = md5.ComputeHash(bytValue);
            var sb = new StringBuilder();
            foreach (var b in bytHash) {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        public static Pair<U,V> make_pair<U,V>(U u, V v) {
            return new Pair<U, V>(u, v);
        }

    }
    class Pair<U, V> {
        public U first { get; set; }
        public V second { get; set; }
        public Pair(U u, V v){
            first = u;
            second = v;
        }
    }
}
