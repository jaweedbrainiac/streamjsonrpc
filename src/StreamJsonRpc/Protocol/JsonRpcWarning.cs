﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using StreamJsonRpc.Reflection;

namespace StreamJsonRpc.Protocol
{
    /// <summary>
    /// Describes the warning resulting from a <see cref="JsonRpcRequest"/> that failed on the server.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class JsonRpcWarning : JsonRpcMessage, IJsonRpcMessageWithId
    {
        /// <summary>
        /// Gets or sets the detail about the warning.
        /// </summary>
        [DataMember(Name = "warning", Order = 2, IsRequired = true)]
        public WarningDetail? Warning { get; set; }

        /// <summary>
        /// Gets or sets an identifier established by the client if a response to the request is expected.
        /// </summary>
        [DataMember(Name = "id", Order = 1, IsRequired = true, EmitDefaultValue = true)]
        public RequestId RequestId { get; set; }

        /// <summary>
        /// Gets the string to display in the debugger for this instance.
        /// </summary>
        protected string DebuggerDisplay => $"Warning: {this.Warning?.Code} \"{this.Warning?.Message}\" ({this.RequestId})";

        /// <inheritdoc/>
        public override string ToString()
        {
            return new JObject
        {
            new JProperty("id", this.RequestId.ObjectValue),
            new JProperty("warning", new JObject
            {
                new JProperty("code", this.Warning?.Code),
                new JProperty("message", this.Warning?.Message),
            }),
        }.ToString(Formatting.None);
        }

        /// <summary>
        /// Describes the warning.
        /// </summary>
        [DataContract]
#pragma warning disable CA1034 // Nested types should not be visible
        public class WarningDetail
#pragma warning restore CA1034 // Nested types should not be visible
        {
            /// <summary>
            /// Gets or sets a number that indicates the warning type that occurred.
            /// </summary>
            /// <value>
            /// The error codes from and including -32768 to -32000 are reserved for errors defined by the spec or this library.
            /// Codes outside that range are available for app-specific error codes.
            /// </value>
            [DataMember(Name = "code", Order = 0, IsRequired = true)]
            public JsonRpcErrorCode Code { get; set; }

            /// <summary>
            /// Gets or sets a short description of the error.
            /// </summary>
            /// <remarks>
            /// The message SHOULD be limited to a concise single sentence.
            /// </remarks>
            [DataMember(Name = "message", Order = 1, IsRequired = true)]
            public object? Message { get; set; }

            /// <summary>
            /// Gets or sets additional data about the error.
            /// </summary>
            [DataMember(Name = "data", Order = 2, IsRequired = false)]
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
#pragma warning disable CA1721 // Property names should not match get methods
            public object? Data { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods

            /// <summary>
            /// Gets the value of the <see cref="Data"/>, taking into account any possible type coercion.
            /// </summary>
            /// <typeparam name="T">The <see cref="Type"/> to coerce the <see cref="Data"/> to.</typeparam>
            /// <returns>The result.</returns>
            public T GetData<T>() => (T)this.GetData(typeof(T))!;

            /// <summary>
            /// Gets the value of the <see cref="Data"/>, taking into account any possible type coercion.
            /// </summary>
            /// <param name="dataType">The <see cref="Type"/> to deserialize the data object to.</param>
            /// <returns>The result.</returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataType"/> is null.</exception>
            /// <remarks>
            /// Derived types may override this method in order to deserialize the <see cref="Data"/>
            /// such that it can be assignable to <paramref name="dataType"/>.
            /// The default implementation does nothing to convert the <see cref="Data"/> object to match <paramref name="dataType"/>, but simply returns the existing object.
            /// Derived types should *not* throw exceptions. This is a best effort method and returning null or some other value is preferable to throwing
            /// as it can disrupt an existing exception handling path.
            /// </remarks>
            public virtual object? GetData(Type dataType)
            {
                Requires.NotNull(dataType, nameof(dataType));
                return this.Data;
            }

            /// <summary>
            /// Provides a hint for a deferred deserialization of the <see cref="Data"/> value as to the type
            /// argument that will be used when calling <see cref="GetData{T}"/> later.
            /// </summary>
            /// <param name="dataType">The type that will be used as the generic type argument to <see cref="GetData{T}"/>.</param>
            /// <remarks>
            /// Overridding methods in types that retain buffers used to deserialize should deserialize within this method and clear those buffers
            /// to prevent further access to these buffers which may otherwise happen concurrently with a call to <see cref="IJsonRpcMessageBufferManager.DeserializationComplete"/>
            /// that would recycle the same buffer being deserialized from.
            /// </remarks>
            protected internal virtual void SetExpectedDataType(Type dataType)
            {
            }
        }
    }
}
