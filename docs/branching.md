# Branching Strategy

[![Branching Strategy](https://mermaid.ink/img/pako:eNqlVMtugzAQ_BW0Z0LA5mXOlXqqVLW3iouDHbAScGRMmzTKvxdI01AcqKJyMt7d2ZmRd4-QScYhgVzoR0V3RVpZ7ZfJshTaEiyxUnh-sTyEU5gK-ZfQStEqK6w1p7pRfEkZW9S6Wa-_CwuebWSjJ-M98I3zpa6kojpflVzl3MQZkgqm-YbToWgkRfEtpzVfIhchJxrxuRk0IOOxO2K_XIl8sWrySX0mCvnh_OOigXO3f1eEQSvsGq1uCZ2F8FKwNM27__dzkeOObCikXnTVOyXZPzSMYYYs0B1C5nCwqcYzoK_ERlNQ8Y-FLkSV1xNjYCTc8_avxUPG_t_0flkRz_kUT7xJHIx9iR13prEJ0A8i2NBKatNYu4WOXWoKuuAlT6FLZFRturRTm0cbLV8PVQaJVg23odkxqvmDoLmiJSRruq3b2x2tIDnCHhIcEsd3PRJFASZuiAIbDpAg4jleRDwcezEOCI6Ckw2fUrYIrkNQ6AcYET8MfULC0AbOhJbq6bwl-2XZt3jrCzoepy-Qd58-?type=png)](https://mermaid.live/edit#pako:eNqlVMtugzAQ_BW0Z0LA5mXOlXqqVLW3iouDHbAScGRMmzTKvxdI01AcqKJyMt7d2ZmRd4-QScYhgVzoR0V3RVpZ7ZfJshTaEiyxUnh-sTyEU5gK-ZfQStEqK6w1p7pRfEkZW9S6Wa-_CwuebWSjJ-M98I3zpa6kojpflVzl3MQZkgqm-YbToWgkRfEtpzVfIhchJxrxuRk0IOOxO2K_XIl8sWrySX0mCvnh_OOigXO3f1eEQSvsGq1uCZ2F8FKwNM27__dzkeOObCikXnTVOyXZPzSMYYYs0B1C5nCwqcYzoK_ERlNQ8Y-FLkSV1xNjYCTc8_avxUPG_t_0flkRz_kUT7xJHIx9iR13prEJ0A8i2NBKatNYu4WOXWoKuuAlT6FLZFRturRTm0cbLV8PVQaJVg23odkxqvmDoLmiJSRruq3b2x2tIDnCHhIcEsd3PRJFASZuiAIbDpAg4jleRDwcezEOCI6Ckw2fUrYIrkNQ6AcYET8MfULC0AbOhJbq6bwl-2XZt3jrCzoepy-Qd58-)

```text
gitGraph
    commit id: "PR 123"
    commit id: "PR 124"
    branch feature/add-stuff
    checkout feature/add-stuff
    commit
    commit
    checkout main
    merge feature/add-stuff id: "PR 125"
    commit id: "PR 126"
    commit id: "PR 127"
    branch release/2022.7
    checkout release/2022.7
    commit id: "PR 128"
    branch fix/big-bug
    checkout main
    commit id: "PR 129"
    checkout fix/big-bug
    commit
    commit
    checkout main
    merge fix/big-bug id: "PR 130"
    checkout release/2022.7
    merge fix/big-bug id: "PR 131" tag: "v2022.7.0"
    branch hot-fix/prod-bug
    commit
    commit
    checkout main
    merge hot-fix/prod-bug id: "PR 132"
    checkout release/2022.7
    merge hot-fix/prod-bug id: "PR 133" tag: "v2022.7.1"
    checkout main
    branch feature/new-things
    checkout feature/new-things
    commit
    checkout main
    merge feature/new-things id: "PR 134"
    checkout main
    branch release/2022.8
    checkout release/2022.8
    commit id: "PR 135" tag: "v2022.8.0"
    checkout main
    commit id: "PR 136"

```
