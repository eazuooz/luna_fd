using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Luna.Gameplay
{
    /// <summary>Addressables asset provider for existing Resources prefab identities. Native GF still loads assets and creates/pools every entity.</summary>
    public sealed class LunaResourcesProvider : ResourceProviderBase
    {
        private const string LocatorName = "Luna Authored Resources";
        public override Type GetDefaultType(IResourceLocation location) => typeof(GameObject);
        public override bool CanProvide(Type type, IResourceLocation location) => type == null || type.IsAssignableFrom(typeof(GameObject));
        public override void Provide(ProvideHandle handle)
        {
            string resource = handle.Location.InternalId;
            ResourceRequest request;
            try { request = Resources.LoadAsync<GameObject>(resource); }
            catch (Exception error) { handle.Complete<GameObject>(null, false, error); return; }
            handle.SetProgressCallback(() => request.progress);
            // Always complete through Unity's async operation. Cached source prefabs do not turn
            // ShowEntity into an immediate GameObject-returning call.
            request.completed += _ =>
            {
                var asset = request.asset as GameObject;
                handle.Complete(asset, asset != null, asset != null ? null : new FileNotFoundException("Resources prefab is missing: " + resource));
            };
        }
        public override void Release(IResourceLocation location, object asset)
        {
            // Resources.UnloadAsset does not accept GameObject assets. These are shared source
            // prefabs referenced by the authored catalog, never instantiated or destroyed here.
            // Unity reclaims unreferenced Resources assets through its normal unused-asset pass.
        }
        public static string ResourcePath(string address)
        {
            const string prefix = "Assets/Resources/";
            if (string.IsNullOrEmpty(address) || !address.StartsWith(prefix, StringComparison.Ordinal)
                || !address.EndsWith(".prefab", StringComparison.Ordinal) || address.Contains("..") || address.Contains("\\"))
                throw new InvalidDataException("Invalid Luna Resources prefab address: " + address);
            return address.Substring(prefix.Length, address.Length - prefix.Length - ".prefab".Length);
        }
        public static ResourceLocationMap CreateLocations(LunaEntityCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var map = new ResourceLocationMap(LocatorName, catalog.Entries.Length);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in catalog.Entries)
            {
                if (!keys.Add(entry.Address)) throw new InvalidDataException("Duplicate entity asset address: " + entry.Address);
                map.Add(entry.Address, new ResourceLocationBase(entry.Address, ResourcePath(entry.Address), typeof(LunaResourcesProvider).FullName, typeof(GameObject)));
            }
            return map;
        }
        public static void Register(LunaEntityCatalog catalog)
        {
            var old = new List<IResourceLocator>();
            foreach (var locator in Addressables.ResourceLocators) if (locator.LocatorId == LocatorName) old.Add(locator);
            foreach (var locator in old) Addressables.RemoveResourceLocator(locator);
            bool providerExists = false;
            foreach (var provider in Addressables.ResourceManager.ResourceProviders)
                if (provider.ProviderId == typeof(LunaResourcesProvider).FullName) { providerExists = true; break; }
            if (!providerExists) Addressables.ResourceManager.ResourceProviders.Add(new LunaResourcesProvider());
            Addressables.AddResourceLocator(CreateLocations(catalog));
        }
    }
}
