---
description: Auto Generate and implement Tagging flow
mode: agent
---
Define the task to achieve:


parameters:
	- name: featureName
		description: The ECS feature name to generate (e.g., checkout, ad_optimization)
		required: true

Define the task to achieve:

Run the following terminal command to generate ECS enum and tracker for the specified feature:

```sh
python3 gen_ecs_enum.py --feature-name {featureName}
```

Specific requirements:
- The script must complete successfully (exit code 0).
- The following files should be generated or updated:
	- ECSCodeGen/ECSEventsCodeGen/{FeatureName}Tracker.swift
	- ECSCodeGen/ECSEventsCodeGen/{FeatureName}EventType.swift

Constraints:
- Only overwrite existing files if necessary and confirmed.
- Use Python 3 for execution.


---
# How to Use

1. Set the `featureName` parameter to the ECS feature you want to generate (e.g., `checkout`, `ad_optimization`).
2. The system will automatically run:
	```sh
	python3 gen_ecs_enum.py --feature-name {featureName}
	```
3. The following files will be generated or updated:
	- `ECSCodeGen/ECSEventsCodeGen/{FeatureName}Tracker.swift`
	- `ECSCodeGen/ECSEventsCodeGen/{FeatureName}EventType.swift`
4. The process will confirm success and check the output files for expected Swift code.

## Example

If you want to generate for the feature `checkout`, set:

```
featureName: checkout
```

The system will run:

```sh
python3 gen_ecs_enum.py --feature-name checkout
```

and verify the output files are correct.
