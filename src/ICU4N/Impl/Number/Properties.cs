using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Numerics
{
    // ICU4N TODO: Serialization

    /// <summary>
    /// ICU 59 called the class DecimalFormatProperties as just Properties. We need to keep a thin implementation for the
    /// purposes of serialization.
    /// </summary>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal class Properties
    {
//        /** Same as DecimalFormatProperties. */
//        private static final long serialVersionUID = 4095518955889349243L;

//        private transient DecimalFormatProperties instance;

//    public DecimalFormatProperties getInstance()
//        {
//            return instance;
//        }

//        private void readObject(ObjectInputStream ois) throws IOException, ClassNotFoundException {
//        if (instance == null) {
//            instance = new DecimalFormatProperties();
//    }
//    instance.readObjectImpl(ois);
//    }

//private void writeObject(ObjectOutputStream oos) throws IOException
//{
//        if (instance == null) {
//        instance = new DecimalFormatProperties();
//    }
//    instance.writeObjectImpl(oos);
//}
    }
}
