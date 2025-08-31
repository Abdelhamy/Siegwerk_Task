# SDS Document Ingestion & Metadata Search System Design

## 1. Overview
This system ingests SDS (Safety Data Sheets) PDFs, extracts structured metadata, and enables fast, secure, and multi-tenant aware search capabilities. The design supports versioning, role-based access control (RBAC), and archiving older versions.

---

## 2. High-Level Architecture Components

- **Upload API**: Issues presigned URLs for secure PDF upload (≤25MB).
- **Object Storage**: Stores raw SDS PDFs (e.g., Azure Blob, S3).
- **Queue**: Event-triggered workflow to decouple upload and processing.
- **Extractor (OCR/ML)**: Parses PDFs and extracts structured metadata.
- **Metadata Writer**: Writes metadata to SQL Server and Outbox table.
- **Indexer**: Indexes key metadata into ElasticSearch/OpenSearch for fast queries.
- **Search API**: Allows users to search by SKU, supplier, docType, and date range.
- **RBAC Service**: Handles access control by tenant and role (Admin, Uploader, Reader).

---

## 3. Event-Driven Flow

```text
Upload → Queue → Extractor → Metadata Writer → Indexer
```

- Upload completes immediately, then queue triggers asynchronous pipeline.
- Metadata and file versioning is enforced before indexing.
- Indexer uses deduplication key to prevent duplicates.

---

## 4. Storage Model

| Layer                   | Usage                         | Technology            |
|------------------------|-------------------------------|------------------------|
| Object Storage         | Raw PDFs                      | Azure Blob / AWS S3   |
| SQL Server             | Structured metadata + version | Relational DB (ACID)  |
| ElasticSearch/OpenSearch | Fast search/filter/indexing  | Secondary search index|

---

## 5. Trade-offs & Justifications

### 📤 Upload: Presigned URL vs Direct POST

| Comparison                  | **Presigned URL (✅ Preferred)**                                         | **Direct POST (via API)**                                                |
|-----------------------------|---------------------------------------------------------------------------|------------------------------------------------------------------------------|
| **Performance**            | ⚡ Faster — bypasses API, direct to blob storage                        | ❌ Slower — file passes through API (Client → API → Blob)               |
| **Resource Cost**          | ✅ Lower — reduces API bandwidth and compute                            | ❌ Higher — API bears full upload load                                  |
| **Security**               | ✅ Secure — temporary URL with size/type constraints                    | ✅ File validation possible before upload                               |
| **Control & Visibility**   | ❌ Less control — no visibility of progress in API                      | ✅ Full control — API can inspect or reject the file                    |
| **Large File Handling**    | ✅ Excellent — direct to blob with no bottleneck                        | ❌ Risk of server overload with large files                             |
| **Observability / Logging**| ❌ Harder — requires blob event hooks or external tools                | ✅ Easier — full control via API flow                                   |
| **Implementation Simplicity** | ✅ Easy — generate temporary URL with constraints                     | ❌ Harder — requires upload handling and error management in API        |
| **Upload Limits**          | ✅ Enforced via URL policy (e.g., 25MB limit)                           | ✅ Enforced manually in API before accepting                            |

### 🔄 Event-Driven Pipeline

| Category              | ✅ Pros                                                            | ❌ Cons                                                       |
|----------------------|-------------------------------------------------------------------|----------------------------------------------------------------|
| Decoupling           | Independent steps → easy to swap, retry, scale                    | Harder to trace end-to-end flow                                |
| Performance          | User upload is fast                                                | Indexing is async (slight delay in searchability)              |
| Resilience           | Retry & DLQ support                                                | Must handle idempotency                                        |
| Observability        | Each step can be logged separately                                 | Requires centralized monitoring                                |

### 📦 Storage Layer Comparison

| Layer                   | ✅ Pros                                                        | ❌ Cons                                                                |
|------------------------|---------------------------------------------------------------|------------------------------------------------------------------------|
| **Object Storage**     | Low cost, scalable, supports presigned URLs                  | No querying, needs OCR/ML, no relational data                        |
| **SQL Server (RDBMS)** | ACID, relational, source of truth, version tracking          | Not suitable for fast/full-text search                              |
| **ElasticSearch**      | Fast, full-text, faceted search, scalable                    | Duplicate data, sync required, no strong consistency                 |

---

## 6. Roles & Access Control (RBAC)

| Role      | Permissions                                                      |
|-----------|------------------------------------------------------------------|
| Admin     | Full access (upload, search, delete/archive, manage settings)   |
| Uploader  | Upload files only                                               |
| Reader    | Search & view metadata                                          |

- OIDC for authentication (e.g., Azure AD B2C)
- Authorization scoped by TenantId and Role.

---

