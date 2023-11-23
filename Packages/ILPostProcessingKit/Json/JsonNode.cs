/**
Copyright (c) 2017 Koki Ibukuro

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// simple access to json
///
/// var json = JsonNode.Parse(jsonString);
/// string foo = json["hoge"][4].Get<string>();
/// </summary>
public class JsonNode : IEnumerable<JsonNode>, IDisposable
{
    object obj;

    public JsonNode(object obj)
    {
        this.obj = obj;
    }

    public void Dispose()
    {
        obj = null;
    }

    public static JsonNode Parse(string json)
    {
        return new JsonNode(MiniJSON.Json.Deserialize(json));
    }

    public JsonNode this [int i]
    {
        get
        {
            if (obj is IList)
            {
                return new JsonNode(((IList)obj)[i]);
            }
            throw new Exception("Object is not IList : " + obj.GetType().ToString());
        }
    }

    public JsonNode this [string key]
    {
        get
        {
            if (obj is IDictionary)
            {
                return new JsonNode(((IDictionary)obj)[key]);
            }
            throw new Exception("Object is not IDictionary : " + obj.GetType().ToString());
        }
    }

    public int Count
    {
        get
        {
            if (obj is IList)
            {
                return ((IList)obj).Count;
            }
            else if (obj is IDictionary)
            {
                return ((IDictionary)obj).Count;
            }
            else
            {
                return 0;
            }
        }
      
    }

    public T Get<T>()
    {
        return (T)obj;
    }

    public IEnumerator<JsonNode> GetEnumerator()
    {
        if (obj is IList)
        {
            foreach (var o in (IList)obj)
            {
                yield return new JsonNode(o);
            }
        }
        else if (obj is IDictionary)
        {
            foreach (var o in (IDictionary)obj)
            {
                yield return new JsonNode(o);
            }
        }
        else
        {
            yield return null;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
