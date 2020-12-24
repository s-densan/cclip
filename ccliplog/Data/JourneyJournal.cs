﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccliplog.Data
{
    class JourneyJournal
    {
    }
}
/*
## データ構造
- meta
  - id
  - creationDate
  - modifiedDate
  - version
  - weather
    - id
    - degree_c
    - description
    - icon
    - place
  - tags*
  - starred
  - location
    - localityName
    - country
    - timeZoneName
    - administrativeArea
    - longitude
    - placeName
    - latitude
- contents
  - text
  - mood
  - type
- attachment
  - photos*
    - type
    - filename
  - audios*
    - type
    - filename


## 参考
### DayOne
- photos*
  - orderInEntry
  - creationDevice
  - duration
  - type
  - filename
  - identifier
  - date
  - height
  - fnumber
  - width
  - md5
  - isSketch
  - focalLength
- creationOSVersion
- starred
- modifiedDate
- editingTime
- creationDate
- creationDeviceModel
- creationDevice
- timeZone
- duration
- tags*
- userActivity
  - activityName
  - stepCount
- richText
- creationOSName
- sourceString
- uuid
- creationDeviceType
- text
- location
  - region
    - center
      - longitude
      - latitude
    - identifier
    - radius
  - localityName
  - country
  - timeZoneName
  - administrativeArea
  - longitude
  - placeName
  - latitude

### Journey

- id
- date_modified
- date_journal
- timezone
- text
- preview_text
- mood
- lat
- lon
- address
- label
- folder
- sentiment
- favourite
- music_title
- music_artist
- photos*
- weather
  - id
  - degree_c
  - description
  - icon
  - place
- tags*
- type

### Memorize
- address
- audio*
- createdDate
- latitude
- longitude
- mood
- moodColor
- moodIcon
- moodName
- photos*
- placeName
- starred
- tags*
- temperature
- text
- weatherCode
- weatherDescription
*/
