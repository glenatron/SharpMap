// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Helper class for serializing System.Drawing.Pen and System.Drawing.Brush
    /// </summary>
    public partial class Surrogates
    {
        /// <summary>
        /// Gets the surrogate selecteds for System.Drawing.Pen and System.Drawing.Brush
        /// </summary>
        /// <returns>SurrogateSelector</returns>
        public static SurrogateSelector GetSurrogateSelectors()
        {
            var sc = new StreamingContext(StreamingContextStates.All);
            var ss = new SurrogateSelector();
            ss.AddSurrogate(typeof (Pen), sc, new PenSurrogate());
            ss.AddSurrogate(typeof (SolidBrush), sc, new SolidBrushSurrogate());
            ss.AddSurrogate(typeof (TextureBrush), sc, new TextureBrushSurrogate());
            ss.AddSurrogate(typeof(HatchBrush), sc, new HatchBrushSurrogate());
            ss.AddSurrogate(typeof(LinearGradientBrush), sc, new LinearGradientBrushSurrogate());
            ss.AddSurrogate(typeof(AdjustableArrowCap), sc, new AdjustableArrowCapSurrogate());
#pragma warning disable 612,618
            ss.AddSurrogate(typeof(CustomLineCap), sc, new CustomLineCapSurrogate());
#pragma warning restore 612,618
            ss.AddSurrogate(typeof(GraphicsPath), sc, new GraphicsPathSurrogate());
            ss.AddSurrogate(typeof(Blend), sc, new BlendSurrogate());
            ss.AddSurrogate(typeof(ColorBlend), sc, new ColorBlendSurrogate());
            ss.AddSurrogate(typeof(Matrix), sc, new MatrixSurrogate());
            return ss;
        }

        #region Nested type: PenSurrogate

        /// <summary>
        /// Surrogate class used for serializing System.Drawing.Pen
        /// </summary>
        public class PenSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// Serialization utility class
            /// </summary>
            [Serializable]
            public class PenRef : IObjectReference, ISerializable
            {
                private readonly Pen _pen;

                /// <summary>
                /// Serialzation constructor
                /// </summary>
                /// <param name="info">The serialization info</param>
                /// <param name="context">The streaming context</param>
                public PenRef(SerializationInfo info, StreamingContext context)
                {
                    var penType = (PenType) info.GetInt32("PenType");
                    var width = info.GetSingle("Width");
                    switch (penType)
                    {
                        case PenType.SolidColor:
                            _pen = new Pen((Color)info.GetValue("Color", typeof(Color)), width);
                            break;
                        default:
                            var brush = (Brush) info.GetValue("Brush", typeof (Brush));
                            _pen = new Pen(brush, width);
                            break;
                    }
                    _pen.Alignment = (PenAlignment)info.GetInt32("Alignment");
                    var ca = (float[]) info.GetValue("CompoundArray", typeof (float[]));
                    if (ca!= null && ca.Length > 0)
                        _pen.CompoundArray = ca;

                    LineCap lineCap;
                    CustomLineCap custonLineCap;
                    DeserializeLineCap(info, "Start", out lineCap, out custonLineCap);
                    _pen.StartCap = lineCap;
                    if (custonLineCap != null) _pen.CustomStartCap = custonLineCap;
                    DeserializeLineCap(info, "End", out lineCap, out custonLineCap);
                    _pen.EndCap = lineCap;
                    if (custonLineCap != null) _pen.CustomEndCap = custonLineCap;

                    /*
                        info.AddValue("DashCap", (int)pen.DashCap);
                        info.AddValue("DashOffset", pen.DashOffset);
                        info.AddValue("DashPattern", pen.DashPattern);
                        info.AddValue("DashStyle", (int)pen.DashStyle);
                     */
                    _pen.DashCap = (DashCap)info.GetInt32("DashCap");
                    _pen.DashOffset = info.GetSingle("DashOffset");
                    var dashStyle = (DashStyle)info.GetInt32("DashStyle");
                    if (dashStyle == DashStyle.Custom)
                        _pen.DashPattern = (float[])info.GetValue("DashPattern", typeof(float[]));
                    else
                        _pen.DashStyle = dashStyle;

                    _pen.LineJoin = (LineJoin)info.GetInt32("LineJoin");
                    _pen.MiterLimit = info.GetSingle("MiterLimit");
                    _pen.Transform = (Matrix)info.GetValue("Transform", typeof(Matrix));

                    System.Diagnostics.Debug.Assert(penType == _pen.PenType);
                }

                private static void DeserializeLineCap(SerializationInfo info, string label, out LineCap lineCap, out CustomLineCap customLineCap)
                {
                    lineCap = (LineCap)info.GetInt32(label + "Cap");
                    customLineCap = null;
                    if (lineCap != LineCap.Custom)
                        return;

                    customLineCap = (CustomLineCap)info.GetValue("Custom" + label + "Cap", typeof(CustomLineCap));
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _pen;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }
            
            #region ISerializationSurrogate Members

            /// <summary>
            /// Populates the provided SerializationInfo with the data needed to serialize the object.
            /// </summary>
            /// <param name="obj">The object to serialize.</param>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            public void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
            {
            
                info.SetType(typeof(PenRef));
                var pen = (Pen) obj;

                info.AddValue("PenType", pen.PenType);
                info.AddValue("Width", pen.Width);
                if (pen.PenType == PenType.SolidColor)
                    info.AddValue("Color", pen.Color);
                else
                    info.AddValue("Brush", pen.Brush);

                info.AddValue("Alignment", (int)pen.Alignment);
                info.AddValue("CompoundArray", pen.CompoundArray);

                var clp = pen.StartCap == LineCap.Custom ? pen.CustomStartCap : null;
                SerializeLineCap(info, "Start", pen.StartCap, clp);
                clp = pen.EndCap == LineCap.Custom ? pen.CustomEndCap : null;
                SerializeLineCap(info, "End",pen.EndCap, clp);

                info.AddValue("DashCap", (int)pen.DashCap);
                info.AddValue("DashOffset", pen.DashOffset);
                info.AddValue("DashStyle", (int)pen.DashStyle);
                if (pen.DashStyle == DashStyle.Custom)
                    info.AddValue("DashPattern", pen.DashPattern);
                info.AddValue("LineJoin", (int)pen.LineJoin);
                info.AddValue("MiterLimit", pen.MiterLimit);
                info.AddValue("Transform", pen.Transform);
            }

            private static void SerializeLineCap(SerializationInfo info, string label, LineCap lineCap,
                                                 CustomLineCap customLineCap)
            {
                if (lineCap == LineCap.Custom)
                {
                    if (CustomEndCapSerializable(customLineCap))
                    {
                        info.AddValue(label + "Cap", (int)lineCap);
                        info.AddValue("Custom" + label + "Cap", customLineCap);
                    }
                    else
                        info.AddValue(label+"Cap", (int)LineCap.Round);
                }
                else
                    info.AddValue(label+"Cap", (int)lineCap);
            }

            private static bool CustomEndCapSerializable(CustomLineCap clp)
            {
                if (clp == null)
                    return false;
                if (clp is AdjustableArrowCap)
                    return true;

                return clp.GetType().IsSerializable;
            }

            /// <summary>
            /// Populates the object using the information in the SerializationInfo
            /// </summary>
            /// <param name="obj">The object to populate.</param>
            /// <param name="info">The information to populate the object.</param>
            /// <param name="context">The source from which the object is deserialized.</param>
            /// <param name="selector">The surrogate selector where the search for a compatible surrogate begins.</param>
            /// <returns></returns>
            public Object SetObjectData(Object obj, SerializationInfo info, StreamingContext context,
                                        ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion

        #region Nested type: AdjustableArrowCapSurrogate
        /// <summary>
        /// Surrogate class used to serialize <see cref="AdjustableArrowCap"/>
        /// </summary>
        public class AdjustableArrowCapSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// Object reference class for <see cref="T:System.Drawing.Drawing2D.AdjustableArrowCap"/>
            /// </summary>
            [Serializable]
            public class AdjustableArrowCapRef : IObjectReference, ISerializable
            {
                private readonly AdjustableArrowCap _aac;

                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info">The serialization info</param>
                /// <param name="context">The streaming context</param>
                public AdjustableArrowCapRef(SerializationInfo info, StreamingContext context)
                {
                    var aac = _aac = new AdjustableArrowCap(info.GetSingle("Width"), info.GetSingle("Height"));
                    aac.BaseCap = (LineCap)info.GetInt32("BaseCap");
                    aac.BaseInset = info.GetSingle("BaseInset");
                    aac.Filled = info.GetBoolean("Filled");
                    aac.MiddleInset = info.GetSingle("MiddleInset");
                    aac.StrokeJoin = (LineJoin)info.GetInt32("StrokeJoin");
                    aac.WidthScale = info.GetSingle("WidthScale");
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _aac;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var aac = (AdjustableArrowCap) obj;
                info.SetType(typeof(AdjustableArrowCapRef));
                info.AddValue("Width", aac.Width);
                info.AddValue("Height", aac.Height);

                info.AddValue("BaseCap", (int)aac.BaseCap);
                info.AddValue("BaseInset", aac.BaseInset);
                info.AddValue("Filled", aac.Filled);
                info.AddValue("MiddleInset", aac.MiddleInset);
                info.AddValue("StrokeJoin", (int)aac.StrokeJoin);
                info.AddValue("WidthScale", aac.WidthScale);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Nested type: CustomLineCapSurrogate
        /// <summary>
        /// Surrogate class used to serialize <see cref="T:System.Drawing.Drawing2D.CustomLineCap"/> objects
        /// </summary>
        /// <remarks>Unfortunatly this does not work, since the</remarks>
        //[Obsolete("Does not work since there is no way to get a hold of the GraphicsPath that make up this object")]

        public class CustomLineCapSurrogate : ISerializationSurrogate
        {
            /// <summary>
            /// Serialization utility class
            /// </summary>
            [Serializable]
            public class CustomLineCapRef : IObjectReference, ISerializable
            {
                private readonly CustomLineCap _clp;
                
                /// <summary>
                /// Serialization constructor
                /// </summary>
                /// <param name="info"></param>
                /// <param name="context"></param>
                public CustomLineCapRef(SerializationInfo info, StreamingContext context)
                {
                    var baseCap = (LineCap) info.GetInt32("BaseCap");
                    var baseInset = info.GetSingle("BaseInset");
                    var fillPath = (GraphicsPath) info.GetValue("FillPath", (typeof (GraphicsPath)));
                    var strokePath = (GraphicsPath)info.GetValue("StrokePath", (typeof(GraphicsPath)));

                    var startCap = (LineCap)info.GetInt32("StartCap");
                    var endCap = (LineCap)info.GetInt32("StartCap");
                    _clp = new CustomLineCap(fillPath, strokePath, baseCap, baseInset);
                    _clp.StrokeJoin = (LineJoin) info.GetInt32("StrokeJoin");
                    _clp.WidthScale = info.GetSingle("WidthScale");
                    _clp.SetStrokeCaps(startCap, endCap);
                }

                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    return _clp;
                }

                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    throw new NotSupportedException();
                }
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                
                var clp = (CustomLineCap)obj;
                info.SetType(typeof(CustomLineCapRef));
                info.AddValue("BaseCap", clp.BaseCap);
                info.AddValue("BaseInset", clp.BaseInset);
                //FillPath
                //No way to get this!
                //StrokePath
                //No way to get this!
                LineCap startCap, endCap;
                clp.GetStrokeCaps(out startCap, out endCap);
                info.AddValue("StartCap", (int)startCap);
                info.AddValue("EndCap", (int)endCap);
                info.AddValue("StrokeJoin", (int)clp.StrokeJoin);
                info.AddValue("WidthScale", clp.WidthScale);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new NotSupportedException();
            }
        }
        #endregion

    }
}