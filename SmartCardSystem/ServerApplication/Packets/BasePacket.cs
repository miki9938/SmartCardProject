using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication.Packets
{
    public class BasePacket<T> where T : class, new()
    {
        protected Configuration Configuration { get; set; }
        protected int endMarkerLength
        {
            get { return Configuration.PacketEndSign.Length; }
        }

        public BasePacket(byte[] packet, Configuration configuration)
        {
            Configuration = configuration;

            int i = packet.Length - 1;
            while (packet[i] == 0)
                --i;
            RawPacket = new byte[i + 1];
            Array.Copy(packet, RawPacket, i + 1);
        }

        //public BasePacket(T packetData, Configuration configuration)
        //{
        //    Configuration = configuration;

        //    Type type = typeof(T);
        //    PacketsTypes packetType;
        //    Enum.TryParse(
        //        type.Name.Substring(0, type.Name.Length - (type.Name.Length - type.Name.IndexOf("PacketData"))),
        //        out packetType);

        //    var byteStream = new MemoryStream();

        //    //Typ pakietu
        //    byteStream.Write(BitConverter.GetBytes((int)packetType), 0, 1);
        //    //Obiekt
        //    byte[] objectRawArray = ObjectToByteArray(packetData);
        //    byteStream.Write(objectRawArray, 0, objectRawArray.Length);
        //    //Znacznik końca
        //    byteStream.Write(Encoding.ASCII.GetBytes(Configuration.PacketEndSign), 0, endMarkerLength);

        //    RawPacket = byteStream.ToArray();
        //}

        public byte[] RawPacket { get; set; }

        protected byte[] RawData
        {
            get { return RawPacket.Skip(1).Take(RawPacket.Length - 1 - endMarkerLength).ToArray(); }
        }

        /// <summary>
        ///     Sprawdza, czy pakiet jest kompletny
        /// </summary>
        /// <returns></returns>
        public bool IsPacketComplete()
        {
            int endMarkerPosition = RawPacket.Length - endMarkerLength;
            byte[] endMarker = RawPacket.Skip(endMarkerPosition).Take(endMarkerLength).ToArray();

            if (Encoding.ASCII.GetString(endMarker) == Configuration.PacketEndSign)
                return true;
            return false;
        }

        //#region Helpers

        ///// <summary>
        /////     Konwertuje obiekt do tablicy bajtów
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //protected byte[] ObjectToByteArray(T obj)
        //{
        //    if (obj == null)
        //        return null;

        //    var binaryStream = new MemoryStream();

        //    List<PropertyInfo> properties =
        //        typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
        //    foreach (PropertyInfo propertyInfo in properties)
        //    {
        //        object data = typeof(T).GetProperty(propertyInfo.Name).GetValue(obj);

        //        switch (propertyInfo.PropertyType.Name)
        //        {
        //            case "String":
        //                byte[] binaryString = Encoding.ASCII.GetBytes((string)data);
        //                binaryStream.Write(binaryString, 0, binaryString.Length);
        //                break;
        //            default:
        //                byte[] binary = ByteArray.GetBytes(data, propertyInfo.PropertyType);
        //                binaryStream.Write(binary, 0, binary.Count());
        //                break;
        //        }

        //        if (properties.IndexOf(propertyInfo) != properties.Count - 1)
        //            binaryStream.Write(Encoding.ASCII.GetBytes(Configuration.PacketSplitSign), 0, dataSplitLength);
        //    }

        //    return binaryStream.ToArray();
        //}

        ///// <summary>
        /////     Konwertuje tablicę bajtów na obiekt
        ///// </summary>
        ///// <param name="array"></param>
        ///// <returns></returns>
        //protected T ByteArrayToObject(byte[] arrBytes)
        //{
        //    List<byte[]> values = ByteArray.SplitByteArray(arrBytes,
        //        Encoding.ASCII.GetBytes(Configuration.PacketSplitSign));
        //    int index = 0;

        //    var result = new T();

        //    var binaryStream = new MemoryStream();

        //    foreach (PropertyInfo propertyInfo in
        //        typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        //    {
        //        switch (propertyInfo.PropertyType.Name)
        //        {
        //            case "String":
        //                propertyInfo.SetValue(result, Encoding.ASCII.GetString(values[index]));
        //                break;
        //            default:
        //                propertyInfo.SetValue(result, ByteArray.ReadBytes(values[index], propertyInfo.PropertyType));
        //                break;
        //        }

        //        index++;
        //    }

        //    return result;
        //}

        //#endregion

        //public T GetPacketDataObject()
        //{
        //    return ByteArrayToObject(RawData);
        //}

    }
}
