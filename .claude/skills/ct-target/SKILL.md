---
name: ct-target
description: Generate a basic iOS API Target following the Requestable protocol. Use when adding a new API endpoint. Each operation is a nested struct inside a [Name]Target container. Targets define httpMethod, params, additionalHeaders, endpoint, and decode(data:). Uses Alamofire + ObjectMapper for response mapping.
---

# iOS Basic API Target Generator

> **Anti-Hallucination:** Verify every symbol, token, path, and identifier against the codebase before generating code. See [ct-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate API Target following the Requestable protocol pattern.

## Input Format

```
TARGET_NAME: <Name, e.g. "UserProfile">
FEATURE: <Module, e.g. "CTUserManagement">
OPERATIONS: <comma-separated, e.g. "get,create,update,delete">
ENTITY: <entity name, e.g. "User">
```

## Single Operation Target Template

```swift
import Foundation
import Action
import Alamofire
import ObjectMapper
import CTCommon
import CTApiClient

struct [Name]Target {
    typealias HTTPMethod = Alamofire.HTTPMethod
    typealias Parameters = Alamofire.Parameters

    struct [Operation]Target: Requestable {
        typealias Output = [ResponseType]?

        // let someID: String
        // let someData: SomeModel

        var httpMethod: HTTPMethod {
            .post // or .get, .put, .delete
        }

        var params: Parameters {
            var params: Parameters = [:]
            // params["key"] = value
            return params
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            "api-endpoint/path"
        }

        func decode(data: Any) -> Output {
            Mapper<[WrapperType]<[ResponseType]>>()
                .map(JSONObject: data)?.data
        }
    }
}
```

## Multiple Operations Template

```swift
import Foundation
import Action
import Alamofire
import ObjectMapper
import CTCommon
import CTApiClient

struct [Name]Target {
    typealias HTTPMethod = Alamofire.HTTPMethod
    typealias Parameters = Alamofire.Parameters

    struct Get[Entity]Target: Requestable {
        typealias Output = [Entity]?

        let entityID: String

        var httpMethod: HTTPMethod { .get }

        var params: Parameters {
            ["id": entityID]
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            "api/[entity]/\(entityID)"
        }

        func decode(data: Any) -> Output {
            Mapper<[WrapperType]<[Entity]>>()
                .map(JSONObject: data)?.data
        }
    }

    struct Create[Entity]Target: Requestable {
        typealias Output = [Entity]?

        let entityData: [Entity]CreateParams

        var httpMethod: HTTPMethod { .post }

        var params: Parameters {
            entityData.toJSON()
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            "api/[entity]"
        }

        func decode(data: Any) -> Output {
            Mapper<[WrapperType]<[Entity]>>()
                .map(JSONObject: data)?.data
        }
    }
}
```

## Common Parameter Patterns

```swift
// GET with optional query params
var params: Parameters {
    var params: Parameters = [:]
    if let filterValue = filterValue {
        params["filter"] = filterValue
    }
    params["page"] = page
    params["limit"] = limit
    return params
}

// POST with optional fields
var params: Parameters {
    var params: Parameters = [:]
    params["owner"] = (UserManager.shared().getUserInfo()?.accountId ?? 0).stringValue
    if let fileID = fileID, !fileID.isEmpty {
        params["file_id"] = fileID
    }
    return params
}

// POST from Mappable model
var params: Parameters {
    entityData.toJSON()
}
```

## Rules

1. All targets are nested structs inside a container `struct [Name]Target`
2. Each operation struct conforms to `Requestable`
3. `Output` typealias is always Optional: `[ResponseType]?`
4. `decode(data:)` uses `Mapper<WrapperType<ResponseType>>().map(JSONObject: data)?.data`
5. `additionalHeaders` defaults to `HTTPConstants.HTTPAcceptHeaders.V1.plain` unless specified
6. No business logic in targets — only HTTP method, params, endpoint, decode
7. Endpoints use string literals (not `Api.*` key lookup — that's added in NetworkHelper separately)
