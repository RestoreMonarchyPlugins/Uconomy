# Uconomy
Basic economy system for in-game currency in Unturned. Updated and added experience sync.

## Credits
Plugin originally created by [fr34kyn01535](https://github.com/fr34kyn01535). We forked it to add some features and update libraries.

## Commands
- `/balance` - Shows your current balance
- `/pay <player> <amount>` - Pay another player

## Permissions
```xml
<Permission Cooldown="0">balance</Permission>
<Permission Cooldown="0">pay</Permission>
```
## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<UconomyConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <DatabaseAddress>localhost</DatabaseAddress>
  <DatabaseUsername>unturned</DatabaseUsername>
  <DatabasePassword>password</DatabasePassword>
  <DatabaseName>unturned</DatabaseName>
  <DatabaseTableName>uconomy</DatabaseTableName>
  <DatabasePort>3306</DatabasePort>
  <InitialBalance>30</InitialBalance>
  <MoneyName>Credits</MoneyName>
  <SyncExperience>false</SyncExperience>
  <SyncIntervalSeconds>5</SyncIntervalSeconds>
</UconomyConfiguration>
```

## Translations
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="command_balance_show" Value="Your current balance is: {0} {1}" />
  <Translation Id="command_pay_invalid" Value="Invalid arguments" />
  <Translation Id="command_pay_error_pay_self" Value="You cant pay yourself" />
  <Translation Id="command_pay_error_invalid_amount" Value="Invalid amount" />
  <Translation Id="command_pay_error_cant_afford" Value="Your balance does not allow this payment" />
  <Translation Id="command_pay_error_player_not_found" Value="Failed to find player" />
  <Translation Id="command_pay_private" Value="You paid {0} {1} {2}" />
  <Translation Id="command_pay_console" Value="You received a payment of {0} {1} " />
  <Translation Id="command_pay_other_private" Value="You received a payment of {0} {1} from {2}" />
</Translations>
```