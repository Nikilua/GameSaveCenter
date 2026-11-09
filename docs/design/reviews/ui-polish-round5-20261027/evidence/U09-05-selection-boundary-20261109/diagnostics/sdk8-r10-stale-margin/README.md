# SDK 8 failed test diagnostic

Frozen source: `3527fb1d9c2c701539ca727a05b2ab6157cd94c3`. The independent Release build completed core tests (157/157) and Worker tests (400 pass, 1 existing skip), then failed in `R10RecentAccessBehaviorTests.SelectedOverviewAndSharedListItemsKeepTheirContentPosition`: the Overview-specific selected frame now has a zero right margin, while this old assertion still expected four DIP. The content-position helper ran before that assertion and passed. The second, shared ListBox margin assertion remains at four DIP and is unchanged.

Local run summary, exact SDK, counts, source blob/hash, TRX hash, drive space and monitor coverage are in `run-summary.json`; original TRX, class console, desktop result and Default desktop window monitor are retained here. The failure is an outdated test expectation; it does not justify changing the geometry or removing the content-position assertion.

GitHub workflow #1498 (`42553664674`) independently failed the same assertion on SDK 8 and SDK 10.0.401. Its diagnostic artifacts remain available as IDs `13646883956` and `13647087889`; details are recorded in the Round5 progress log after the fix is validated.
