---
description: "Generate basic iOS API Target structure"
mode: "agent"
---

# iOS Basic API Target Generator

Generate basic API Target following Requestable protocol patterns.

## Instructions

Reference our C#/.NET WPF development guidelines: [iOS Guidelines](../instructions/ios-general-instructions.instructions.md)

Generate basic API Target structure with:

-   Requestable protocol conformance
-   Proper HTTP method and parameters
-   Endpoint definition
-   Decode method implementation
-   TODO comments for implementation

## Target Template

```swift
import Foundation
import Action
import Alamofire
import ObjectMapper
import AppCommon
import CTApiClient

struct [Name]Target {
    typealias HTTPMethod = Alamofire.HTTPMethod
    typealias Parameters = Alamofire.Parameters
    typealias Dictionary = [String: Any]

    struct [Operation]Target: Requestable {
        typealias Output = [ResponseType]?

        // TODO: Add input parameters
        // let someID: String
        // let someData: SomeModel

        var httpMethod: HTTPMethod {
            // TODO: Choose appropriate HTTP method
            .post // or .get, .put, .delete
        }

        var params: Parameters {
            // TODO: Build request parameters
            var params: Parameters = [:]
            // params["key"] = value
            return params
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            // TODO: Set appropriate headers (use [SKIP] if no custom headers needed)
            // [SKIP] // Most APIs don't need custom headers
            HTTPConstants.HTTPAcceptHeaders.V1.plain
        }

        var endpoint: String {
            // TODO: Define API endpoint
            "api-endpoint/path"
        }

        func decode(data: Any) -> Output {
            // TODO: Implement response mapping
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
import AppCommon
import CTApiClient

struct [Name]Target {
    typealias HTTPMethod = Alamofire.HTTPMethod
    typealias Parameters = Alamofire.Parameters
    typealias Dictionary = [String: Any]

    struct Get[Entity]Target: Requestable {
        typealias Output = [Entity]?

        let entityID: String

        var httpMethod: HTTPMethod { .get }

        var params: Parameters {
            [
                "id": entityID
            ]
        }

        // **Default Skip**
        var additionalHeaders: Alamofire.HTTPHeaders {
            // TODO: Set appropriate headers (use [SKIP] if no custom headers needed)
            // [SKIP] // Most APIs don't need custom headers
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
            // TODO: Set appropriate headers (use [SKIP] if no custom headers needed)
            // [SKIP] // Most APIs don't need custom headers
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

    struct Update[Entity]Target: Requestable {
        typealias Output = [Entity]?

        let entityID: String
        let entityData: [Entity]UpdateParams

        var httpMethod: HTTPMethod { .put }

        var params: Parameters {
            entityData.toJSON()
        }

        var additionalHeaders: Alamofire.HTTPHeaders {
            // TODO: Set appropriate headers (use [SKIP] if no custom headers needed)
            // [SKIP] // Most APIs don't need custom headers
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
}
```

## Template Variables

-   `${input:targetName}`: Target name (e.g., "UserProfile")
-   `${input:feature}`: Feature module (e.g., "CTUserManagement")
-   `${input:operations}`: Comma-separated operations (e.g., "get,create,update,delete")
-   `${input:entityName}`: Entity name (e.g., "User")

## Usage Examples

-   `/ios-target targetName:UserProfile feature:CTUserManagement operations:get,create entityName:User`
-   `/ios-target targetName:RapidListing feature:CTEcommerce operations:getSignedURLs,makeInference`

## Common Patterns

### GET Request with Query Parameters

```swift
var params: Parameters {
    var params: Parameters = [:]
    if let filterValue = filterValue {
        params["filter"] = filterValue
    }
    params["page"] = page
    params["limit"] = limit
    return params
}
```

### POST Request with File Upload

```swift
var params: Parameters {
    var params: Parameters = [:]
    params["owner"] = (UserManager.shared().getUserInfo()?.accountId ?? 0).stringValue
    if let fileID = fileID, !fileID.isEmpty {
        params["file_id"] = fileID.withSuffix()
    }
    return params
}
```

### Conditional Parameters

```swift
var params: Parameters {
    var params: Parameters = [:]
    if let videoFileID = videoFileID, !videoFileID.isEmpty {
        params["video_file_id"] = videoFileID.withMP4Suffix()
    }
    if !imageFileIDs.isEmpty {
        params["image_file_ids"] = imageFileIDs
    }
    return params
}
```

## Output

Generate basic API Target with:

1. Proper imports (Foundation, Alamofire, ObjectMapper, AppCommon, CTApiClient)
2. Requestable protocol conformance
3. HTTP method and parameters configuration
4. Endpoint definition
5. Response mapping with ObjectMapper
6. TODO comments for implementation
7. Proper naming conventions

Keep implementation minimal with TODO guidance for API-specific logic.
