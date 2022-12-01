// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Helpers
{
    /// <summary>
    /// Tokens are used inside the client for data validation.
    /// We have a collision between MHDR/MODF, not sure, thats what the client checks.
    /// </summary>
    public static class Tokens
    {
        public static uint MVER = 1297499474; // ADT version, 18 just as all others
        public static uint MPHD = 1297107012; // SMMapHeader
        public static uint MAIN = 1296124238; // Map tile table. Needs to contain 64x64 = 4096 entries of sizeof(SMAreaInfo)
        public static uint MHDR = 1297040454; // The start of what is now the ADT files.
        public static uint MHDRChunk = 1296581714;
        public static uint MCIN = 1296255310; // (256 Entries, so a 16*16 Chunkmap.
        public static uint MTEX = 1297368408; // List of textures used for texturing the terrain in this map tile.
        public static uint MDDF = 1296319558; // Placement information for doodads (MDX models). Additional to this, the models to render are referenced in each MCRF chunk.
        public static uint MODF = 1297040454; // It is usually used by WMO based maps which contain no ADT parts with the exception of RazorfenDowns.
        public static uint MCNK = 1296256587; // The header is 128 bytes like later versions, but information inside is placed slightly differently. Offsets are relative to the end of MCNK header.
        public static uint MDNM = 1296322125; // Filenames Doodads. Zero-terminated strings with complete paths to models.
        public static uint MONM = 1297043021; // Filenames WMOS. Zero-terminated strings with complete paths to models.
        public static uint MCLY = 1296256089; // Texture layer definitions for this map chunk. 16 bytes per layer, up to 4 layers (thus, layer count = size / 16).
        public static uint MCRF = 1296257606; // A list of with MCNK.nDoodadRefs + MCNK.nMapObjRefs indices into the file's MDDF and MODF chunks, saying which MCNK subchunk those particular doodads and objects are drawn within. This MCRF list contains duplicates for map doodads that overlap areas.

        // WMO
        // MVER First
        public static uint MOMO = 1297042767; // Rather than all chunks being top level, they have been wrapped in MOMO. There has been no other additional data, rather than just everything being wrapped.
        public static uint MOHD = 1297041476; // Header for the map object. 64 bytes.
        public static uint MOTX = 1297044568; // List of textures (BLP Files) used in this map object.
        public static uint MOMT = 1297042772; // Materials used in this map object, 64 bytes per texture (BLP file).
        public static uint MOGN = 1297041230; // List of group names for the groups in this map object. 
        public static uint MOGI = 1297041225; // Group information for WMO groups, 32 bytes per group, nGroups entries.
        public static uint MOPV = 1297043542; // Portal vertices, one entry is a float[3], usually 4 * 3 * float per portal (actual number of vertices given in portal entry)
        public static uint MOPT = 1297043540; // Portal information. 20 bytes per portal, nPortals entries. There is a hardcoded maximum of 128 portals in a single WMO.
        public static uint MOPR = 1297043538; // Map Object Portal References from groups. Mostly twice the number of portals. Actual count defined by sum (MOGP.portals_used).
        public static uint MOLT = 1297042516; // Lighting information. 48 bytes per light, nLights entries
        public static uint MODS = 1297040467; // This chunk defines doodad sets. There are 32 bytes per doodad set
        public static uint MODN = 1297040462; // List of filenames for Mdx models that appear in this WMO.
        public static uint MODD = 1297040452; // Information for doodad instances. 40 bytes per doodad instance, nDoodads entries.
        public static uint MFOG = 1296453447; // Fog information. Made up of blocks of 48 bytes.
        public static uint MOGP = 1297041232; // WMO group.
        public static uint MCVP = 1296258640; // Convex Volume Planes. Contains blocks of floating-point numbers. 0x10 bytes (4 floats) per entry.

        // WMOGroup (Required)
        public static uint MOPY = 1297043545; // Material info for triangles, two bytes per triangle. So size of this chunk in bytes is twice the number of triangles in the WMO group.
        public static uint MOVT = 1297045076; // Vertices chunk., count = size / (sizeof(float) * 3). 3 floats per vertex, the coordinates are in (X,Z,-Y) order.
        public static uint MONR = 1297043026; // Normals. count = size / (sizeof(float) * 3). 3 floats per vertex normal, in (X,Z,-Y) order.
        public static uint MOTV = 1297044566; // Texture coordinates, 2 floats per vertex in (X,Y) order. The values usually range from 0.0 to 1.0, but it's ok to have coordinates out of that range.
        public static uint MOLV = 1297042518; // This chunk is referenced by MOPY index with 3 entries per SMOPoly.
        public static uint MOIN = 1297041742; // It's most of the time only a list incrementing from 0 to nFaces * 3 or less, not always up to nPolygons (calculated with MOPY)
        public static uint MOBA = 1297039937; // Render batches. Records of 24 bytes.

        // WMOGroup (Optional)
        public static uint MOLR = 1297042514; // Light references, one 16-bit integer per light reference.
        public static uint MODR = 1297040466; // Doodad references, one 16-bit integer per doodad.
        public static uint MOBN = 1297039950; // Nodes of the BSP tree, used for collision (along with bounding boxes ?).
        public static uint MOBR = 1297039954; // Face indices for CAaBsp (MOBN). Triangle indices (in MOVI which define triangles) to describe polygon planes defined by MOBN BSP nodes.
        public static uint MPBV = 1297105494; // These chunks are barely ever present (the one file known is StonetalonWheelPlatform.wmo from alpha).
        public static uint MPBP = 1297105488; // uint16_t mpbv[];
        public static uint MPBI = 1297105481; // uint16_t mpb_indices[]; Triangle vertex indices into into #MPBG
        public static uint MPBG = 1297105479; // C3Vectorⁱ mpb_vertices[];
        public static uint MOCV = 1297040214; // Vertex colors, 4 bytes per vertex (BGRA), for WMO groups using indoor lighting.
        public static uint MLIQ = 1296845137; // Specifies liquids inside WMOs.

        // WMOGroup Lights
        public static uint MOLM = 1297042509; // Lightmaps were the original lighting implementation for WMOs and the default light mode used in the alpha clients.
        public static uint MOLD = 1297042500; // This chunk stores a 255x255ᵘ DXT1 compressed colour palette.

        // MDX TAGS
        public static uint TEXS = 1398293844; // Textures. The client reads MDLTEXTURESECTIONs until chunk.size bytes have been read.
        public static uint BONE = 1162760002;
        public static uint CLID = 1145654339; // Collision. VRTX vertices, TRI Triangles & NRMS (Normals)
        public static uint TRI =  541676116;  // Triangles used alongside CLID. 
        public static uint HTST = 1414747208; // Hit test shapes.
        public static uint TXAN = 1312905300; // Texture Animations.
        public static uint MODL = 1279545165; // Global model information.
        public static uint SEQS = 1397835091; // Sequences. MDX uses a single track for all animations meaning start times and end times between each animation are consecutive.
        public static uint PIVT = 1414941008; // Pivot points. The client reads C3Vectors until chunk.size bytes have been read. PivotPoints are paired with MDLGENOBJECTs by matching indices.
        public static uint PRE2 = 843403856;  // Particle Emitter 2
        public static uint CAMS = 1397571907; // Cameras.
        public static uint LITE = 1163151692; // Lights.
        public static uint RIBB = 1111640402; // Ribbon emitter.
        public static uint MTLS = 1397511245; // Materials
        public static uint GEOS = 1397704007; // Geosets.
        public static uint GEOA = 1095714119;
        public static uint ATCH = 1212372033; // Attachment Points.
        public static uint HELP = 1347175752;
        public static uint EVTS = 1398036037; // Events. 
        public static uint GLBS = 1396853831; // Maximum lengths for sequence ranges. This chunk has no count, the client reads uint32_ts until chunk.size bytes have been read.
    }
}
