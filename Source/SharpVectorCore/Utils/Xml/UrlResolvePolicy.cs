﻿using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpVectors.Xml
{
    public class UrlResolvePolicy
    {
        private readonly DtdProcessing _processing;
        private readonly UrlResolveTypes _entity;
        private readonly UrlResolveTypes _element;
        private readonly UrlResolveTypes _document;
        private readonly UrlResolveTypes _font;
        private readonly UrlResolveTypes _image;
        private readonly UrlResolveTypes _style;
        private readonly UrlResolveTypes _script;

        public UrlResolvePolicy(DtdProcessing processing)
        {
            _processing = processing;
            _entity     = processing == DtdProcessing.Parse ? UrlResolveTypes.Resource : UrlResolveTypes.None;
            _element    = UrlResolveTypes.Local | UrlResolveTypes.Remote;
            _document   = UrlResolveTypes.Local | UrlResolveTypes.Remote;
            _font       = UrlResolveTypes.Local | UrlResolveTypes.Remote;
            _image      = UrlResolveTypes.Local | UrlResolveTypes.Remote;
            _style      = UrlResolveTypes.Local | UrlResolveTypes.Remote;
            _script     = UrlResolveTypes.Local | UrlResolveTypes.Remote;
        }

        public UrlResolvePolicy(UrlResolveArgs args)
        {
            _processing = args.Processing;
            _entity     = args.Entity;
            _element    = args.Element;
            _document   = args.Document;
            _font       = args.Font;
            _image      = args.Image;
            _style      = args.Style;
            _script     = args.Script;
        }

        public DtdProcessing Processing { get => _processing; }
        public UrlResolveTypes Entity { get => _entity; }
        public UrlResolveTypes Element { get => _element; }
        public UrlResolveTypes Document { get => _document; }
        public UrlResolveTypes Font { get => _font; }
        public UrlResolveTypes Image { get => _image; }
        public UrlResolveTypes Style { get => _style; }
        public UrlResolveTypes Script { get => _script; }

        public virtual bool Supports(Uri uri, UrlResolveSource source)
        {
            if (uri == null || !uri.IsAbsoluteUri)
            {
                return false;
            }
            UrlResolveTypes types = UrlResolveTypes.None;
            switch (source)
            {
                case UrlResolveSource.Entity:
                    types = this.Entity;
                    break;
                case UrlResolveSource.Element:
                    break;
                case UrlResolveSource.Document:
                    break;
                case UrlResolveSource.Font:
                    break;
                case UrlResolveSource.Image:
                    break;
                case UrlResolveSource.Style:
                    break;
                case UrlResolveSource.Script:
                    break;
            }
            if (uri.Scheme.StartsWith("pack") && types.HasFlag(UrlResolveTypes.Resource))
            {
                return true;
            }
            if (uri.IsFile && types.HasFlag(UrlResolveTypes.Local))
            {
                return true;
            }

            return !uri.IsFile && types.HasFlag(UrlResolveTypes.Remote);
        }

        /// <summary>
        /// Maps a URI to an object that contains the actual resource.
        /// </summary>
        /// <param name="absoluteUri">The URI returned from <see cref="ResolveUri(Uri, string)"/>.</param>
        /// <param name="role">Currently not used.</param>
        /// <param name="ofObjectToReturn">The type of object to return. The current implementation only returns <see cref="Stream"/> objects.</param>
        /// <returns>A stream object or <see langword="null"/> if a type other than stream is specified.</returns>
        public virtual object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            return null;
        }

        /// <summary>
        /// Resolves the absolute URI from the base and relative URIs.
        /// </summary>
        /// <param name="baseUri">The base URI used to resolve the relative URI.</param>
        /// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative. 
        /// If absolute, this value effectively replaces the <paramref name="baseUri"/> value. 
        /// If relative, it combines with the <paramref name="baseUri"/> to make an absolute URI.</param>
        /// <returns>
        /// A <see cref="T:System.Uri"/> representing the absolute URI, or <see langword="null"/> if the relative URI cannot be resolved.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="baseUri"/>is null or <paramref name="relativeUri"/> is null</exception>
        public virtual Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return null;
        }
    }
}
