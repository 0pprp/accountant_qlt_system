<template>
  <div class="df-page" dir="rtl">
    <header class="df-head">
      <div>
        <h1 class="df-title">{{ $t('delegateFollowup.title') }}</h1>
        <p v-if="currentBranch" class="df-subtitle">
          {{ $t('delegateFollowup.branch.current') }}:
          {{ currentBranch.name }}
          (<span class="df-num">#{{ currentBranch.id }}</span>)
        </p>
      </div>
    </header>

    <!-- 1. Branch picker -->
    <section v-if="branchId == null" class="df-section" aria-labelledby="df-branch-title">
      <h2 id="df-branch-title" class="df-section-title">
        {{ $t('delegateFollowup.branch.pickTitle') }}
      </h2>
      <p class="df-muted">{{ $t('delegateFollowup.branch.pickHint') }}</p>
      <div v-if="!branches.length" class="df-empty">
        {{ $t('delegateFollowup.branch.none') }}
      </div>
      <div v-else class="df-grid">
        <article
          v-for="b in branches"
          :key="b.id"
          class="df-card"
          :class="{ selected: auth.selectedBranch?.id === b.id }"
        >
          <div class="df-card-top">
            <div>
              <h3 class="df-card-name">{{ b.name }}</h3>
              <span class="df-card-id">
                {{ $t('delegateFollowup.branch.id') }}:
                <span class="df-num">#{{ b.id }}</span>
              </span>
            </div>
          </div>
          <button type="button" class="df-card-btn" @click="pickBranch(b.id)">
            <span>{{ $t('delegateFollowup.branch.open') }}</span>
            <span aria-hidden="true">←</span>
          </button>
        </article>
      </div>
    </section>

    <!-- 2. Lists -->
    <template v-else-if="listId == null">
      <section class="df-section" aria-labelledby="df-lists-title">
        <div class="df-section-head">
          <h2 id="df-lists-title" class="df-section-title">
            {{ $t('delegateFollowup.lists.title') }}
          </h2>
          <button type="button" class="df-btn ghost" @click="changeBranch">
            {{ $t('delegateFollowup.branch.change') }}
          </button>
        </div>

        <div v-if="listsQuery.isLoading.value" class="df-muted" role="status">
          {{ $t('delegateFollowup.lists.loading') }}
        </div>
        <div v-else-if="listsQuery.isError.value" class="df-error" role="alert">
          <span>{{ $t('delegateFollowup.lists.error') }}</span>
          <button type="button" class="df-btn ghost" @click="listsQuery.refetch()">
            {{ $t('delegateFollowup.lists.retry') }}
          </button>
        </div>
        <div v-else-if="!lists.length" class="df-empty">
          {{ $t('delegateFollowup.lists.empty') }}
        </div>
        <div v-else class="df-grid">
          <article v-for="l in lists" :key="l.listId" class="df-card">
            <div class="df-card-top">
              <div>
                <h3 class="df-card-name">{{ l.listName }}</h3>
                <span class="df-card-id">
                  <span class="df-num">#{{ l.listId }}</span>
                </span>
              </div>
              <span class="df-badge" :class="tierClass(l.realRatingLabel)">
                {{ l.realRatingLabel }}
                <span class="df-num">{{ fmtNum(l.realRating) }}</span>
              </span>
            </div>
            <dl class="df-meta">
              <div>
                <dt>{{ $t('delegateFollowup.lists.nonZeroed') }}</dt>
                <dd>
                  <span class="df-num">{{ l.nonZeroedCustomerCount }}</span>
                  {{ $t('delegateFollowup.lists.customerUnit') }}
                </dd>
              </div>
              <div>
                <dt>{{ $t('delegateFollowup.lists.lastPayment') }}</dt>
                <dd><span class="df-num">{{ l.lastPaymentDate ?? '—' }}</span></dd>
              </div>
              <div v-if="l.delegateName">
                <dt>{{ $t('delegateFollowup.lists.delegate') }}</dt>
                <dd>{{ l.delegateName }}</dd>
              </div>
              <div v-if="l.followerName">
                <dt>{{ $t('delegateFollowup.lists.follower') }}</dt>
                <dd>{{ l.followerName }}</dd>
              </div>
            </dl>
            <button type="button" class="df-card-btn" @click="openList(l.listId)">
              <span>{{ $t('delegateFollowup.lists.open') }}</span>
              <span aria-hidden="true">←</span>
            </button>
          </article>
        </div>
      </section>
    </template>

    <!-- 3. Detail -->
    <template v-else>
      <header class="df-section">
        <div class="df-section-head">
          <div>
            <h2 class="df-section-title">
              {{ summary?.listName ?? selectedListName }}
            </h2>
            <p v-if="summary" class="df-muted">
              {{ $t('delegateFollowup.detail.asOf') }}
              <span class="df-num">{{ summary.asOfDate }}</span>
              · <span class="df-num">#{{ summary.listId }}</span>
            </p>
          </div>
          <div class="df-actions">
            <button type="button" class="df-btn ghost" @click="changeBranch">
              {{ $t('delegateFollowup.branch.change') }}
            </button>
            <button type="button" class="df-btn ghost" @click="backToLists">
              {{ $t('delegateFollowup.detail.back') }}
            </button>
            <button
              type="button"
              class="df-btn primary"
              :aria-expanded="showTable"
              aria-controls="df-customers"
              @click="toggleTable"
            >
              {{
                showTable
                  ? $t('delegateFollowup.detail.hideDetails')
                  : $t('delegateFollowup.detail.showDetails')
              }}
            </button>
            <button
              type="button"
              class="df-btn ghost"
              @click="openFullCustomersPage"
            >
              {{ $t('delegateFollowup.customersPage.title') }}
            </button>
          </div>
        </div>
      </header>

      <div v-if="summaryQuery.isLoading.value" class="df-muted" role="status">
        {{ $t('delegateFollowup.detail.loading') }}
      </div>
      <div v-else-if="summaryQuery.isError.value" class="df-error" role="alert">
        <span>{{ $t('delegateFollowup.detail.error') }}</span>
        <button type="button" class="df-btn ghost" @click="reloadDetail">
          {{ $t('delegateFollowup.lists.retry') }}
        </button>
      </div>

      <div v-else-if="summary" class="df-detail">
        <!-- 8. Unresolved notes -->
        <aside v-if="unresolvedNotes.length" class="df-banner" role="note">
          <h3>{{ $t('delegateFollowup.detail.unresolvedTitle') }}</h3>
          <ul>
            <li v-for="(n, i) in unresolvedNotes" :key="i">
              <strong>{{ n.field }}</strong>: {{ n.note }}
            </li>
          </ul>
        </aside>

        <!-- Ratings -->
        <section class="df-section" aria-labelledby="df-ratings-title">
          <h2 id="df-ratings-title" class="df-section-title">
            {{ $t('delegateFollowup.ratings.title') }}
          </h2>
          <div class="df-tiles">
            <!-- real rating + trend -->
            <div class="df-tile">
              <span class="df-tile-label">{{ $t('delegateFollowup.ratings.real') }}</span>
              <span class="df-tile-value">
                <span class="df-num">{{ fmtNum(summary.realRating) }}</span>
                <span class="df-badge" :class="tierClass(summary.realRatingLabel)">
                  {{ summary.realRatingLabel }}
                </span>
              </span>
              <span class="df-trend" :class="trendKey">
                {{ trendIcon }} {{ trendText }}
                · {{ $t('delegateFollowup.ratings.weekAgo') }}:
                <span class="df-num">{{ fmtNum(summary.realRatingWeekAgo) }}</span>
              </span>
              <button
                type="button"
                class="df-btn ghost small"
                @click="openRealHistory"
              >
                {{ $t('delegateFollowup.ratings.history') }}
              </button>
            </div>

            <!-- board operator -->
            <div class="df-tile">
              <span class="df-tile-label">{{ $t('delegateFollowup.ratings.board') }}</span>
              <template v-if="summary.boardOperatorRating.hasRating">
                <span class="df-tile-value">
                  <span class="df-num">{{ summary.boardOperatorRating.score ?? '—' }}</span>
                  <span class="df-muted">/ 10</span>
                </span>
                <span class="df-tile-note">
                  {{ summary.boardOperatorRating.note || $t('delegateFollowup.ratings.noNote') }}
                </span>
                <span class="df-muted">
                  <template v-if="summary.boardOperatorRating.ratedByUserName">
                    {{ $t('delegateFollowup.ratings.ratedBy') }}
                    {{ summary.boardOperatorRating.ratedByUserName }}
                  </template>
                  {{ $t('delegateFollowup.ratings.ratedAt') }}
                  <span class="df-num">{{ summary.boardOperatorRating.ratingDate }}</span>
                </span>
              </template>
              <span v-else class="df-tile-note df-muted">
                {{ $t('delegateFollowup.ratings.boardNone') }}
              </span>
              <div class="df-actions">
                <button
                  v-if="canRate"
                  type="button"
                  class="df-btn primary small"
                  @click="rateOpen = true"
                >
                  {{
                    summary.boardOperatorRating.hasRating
                      ? $t('delegateFollowup.ratings.rateAgain')
                      : $t('delegateFollowup.ratings.rate')
                  }}
                </button>
                <button
                  type="button"
                  class="df-btn ghost small"
                  @click="openBoardHistory"
                >
                  {{ $t('delegateFollowup.ratings.history') }}
                </button>
              </div>
            </div>

            <!-- committees -->
            <div
              v-for="c in committees"
              :key="c.key"
              class="df-tile"
              :class="{ unresolved: !c.data.available }"
            >
              <span class="df-tile-label">{{ c.label }}</span>
              <template v-if="c.data.available">
                <template v-if="c.data.latest?.hasRating">
                  <span class="df-tile-value">
                    <span class="df-num">{{ c.data.latest.score ?? '—' }}</span>
                  </span>
                  <span class="df-tile-note">
                    {{ c.data.latest.note || $t('delegateFollowup.ratings.noNote') }}
                  </span>
                </template>
                <span v-else class="df-tile-note df-muted">
                  {{ $t('delegateFollowup.ratings.boardNone') }}
                </span>
                <button
                  type="button"
                  class="df-btn ghost small"
                  @click="openCommitteeHistory(c.label, c.data)"
                >
                  {{ $t('delegateFollowup.ratings.history') }}
                </button>
              </template>
              <template v-else>
                <span class="df-tile-value">{{ $t('delegateFollowup.ratings.unresolved') }}</span>
                <span class="df-tile-note">{{ c.data.note }}</span>
              </template>
            </div>
          </div>
        </section>

        <!-- Income -->
        <section class="df-section" aria-labelledby="df-income-title">
          <h2 id="df-income-title" class="df-section-title">
            {{ $t('delegateFollowup.income.title') }}
          </h2>
          <div class="df-tiles">
            <div class="df-tile">
              <span class="df-tile-label">{{ $t('delegateFollowup.income.requiredToday') }}</span>
              <span
                v-if="summary.requiredDueAvailable"
                class="df-tile-value"
              >{{ fmtMoney(summary.requiredDueToday ?? 0) }}</span>
              <span
                v-else
                class="df-tile-value df-unavailable"
                :title="summary.requiredDueUnavailableReason ?? undefined"
              >{{ $t('delegateFollowup.income.requiredUnavailable') }}</span>
            </div>
            <div class="df-tile">
              <span class="df-tile-label">{{ $t('delegateFollowup.income.collectedYesterday') }}</span>
              <span class="df-tile-value">{{ fmtMoney(summary.collectedYesterday) }}</span>
              <button type="button" class="df-btn ghost small" @click="openDaysHistory">
                {{ $t('delegateFollowup.income.historyDays') }}
              </button>
            </div>
            <div class="df-tile">
              <span class="df-tile-label">
                {{ $t('delegateFollowup.income.collectedLastFriday') }}
                (<span class="df-num">{{ summary.lastFridayDate }}</span>)
              </span>
              <span class="df-tile-value">{{ fmtMoney(summary.collectedLastFriday) }}</span>
              <button type="button" class="df-btn ghost small" @click="openFridaysHistory">
                {{ $t('delegateFollowup.income.historyFridays') }}
              </button>
            </div>
          </div>

          <template v-if="summary.tierMoney.length">
            <h3 class="df-chip-group-title">{{ $t('delegateFollowup.income.tiers') }}</h3>
            <div class="df-table-wrap">
              <table class="df-table">
                <thead>
                  <tr>
                    <th scope="col">{{ $t('delegateFollowup.income.tier') }}</th>
                    <th scope="col">{{ $t('delegateFollowup.income.required') }}</th>
                    <th scope="col">{{ $t('delegateFollowup.income.collected') }}</th>
                    <th scope="col">{{ $t('delegateFollowup.ratings.history') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="tm in summary.tierMoney" :key="tm.label">
                    <td>
                      <span class="df-badge" :class="tierClass(tm.label)">{{ tm.label }}</span>
                    </td>
                    <td class="nowrap">
                      <template v-if="tm.requiredDueAvailable">
                        {{ fmtMoney(tm.requiredDue ?? 0) }}
                      </template>
                      <span v-else class="df-unavailable">—</span>
                    </td>
                    <td class="nowrap">{{ fmtMoney(tm.collectedYesterday) }}</td>
                    <td>
                      <button
                        type="button"
                        class="df-btn ghost small"
                        @click="openTierHistory(tm)"
                      >
                        {{ $t('delegateFollowup.ratings.history') }}
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </template>
        </section>

        <!-- Non-zeroed -->
        <section class="df-section" aria-labelledby="df-unzeroed-title">
          <div class="df-section-head">
            <h2 id="df-unzeroed-title" class="df-section-title">
              {{ $t('delegateFollowup.unzeroed.title') }}
              ·
              <span class="df-num">{{ summary.nonZeroedCustomerCount }}</span>
            </h2>
          </div>
          <p class="df-muted">{{ $t('delegateFollowup.unzeroed.hint') }}</p>

          <div>
            <h3 class="df-chip-group-title">{{ $t('delegateFollowup.unzeroed.tiersGroup') }}</h3>
            <div class="df-chip-row">
              <button
                v-for="tc in tierChips"
                :key="tc.label"
                type="button"
                class="df-chip"
                :aria-pressed="tableFilter === 'tiers' && tierFilter === tc.label"
                @click="toggleTierChip(tc.label)"
              >
                {{ tc.label }}
                <strong class="df-num">{{ tc.count }}</strong>
              </button>
              <button
                type="button"
                class="df-chip"
                :class="{ disabled: !legalAvailable }"
                :aria-pressed="tableFilter === 'legal'"
                :disabled="!legalAvailable"
                :title="legalAvailable ? undefined : $t('delegateFollowup.unzeroed.legalUnavailable')"
                @click="toggleFilter('legal')"
              >
                {{ $t('delegateFollowup.unzeroed.legal') }}
                <strong class="df-num">
                  {{ legalAvailable ? (summary.legalCustomerCount ?? 0) : '—' }}
                </strong>
              </button>
            </div>
            <p v-if="!legalAvailable" class="df-muted">
              {{ $t('delegateFollowup.unzeroed.legalUnavailable') }}
            </p>
          </div>

          <div>
            <h3 class="df-chip-group-title">{{ $t('delegateFollowup.unzeroed.remainingGroup') }}</h3>
            <div class="df-chip-row">
              <button
                type="button"
                class="df-chip"
                :aria-pressed="tableFilter === 'lt100'"
                @click="toggleFilter('lt100')"
              >
                {{ $t('delegateFollowup.unzeroed.lt100') }}
                <strong class="df-num">{{ summary.remainingLt100Count }}</strong>
              </button>
              <button
                type="button"
                class="df-chip"
                :aria-pressed="tableFilter === '100to200'"
                @click="toggleFilter('100to200')"
              >
                {{ $t('delegateFollowup.unzeroed.range100200') }}
                <strong class="df-num">{{ summary.remaining100To200Count }}</strong>
              </button>
              <span class="df-chip static" :title="$t('delegateFollowup.unzeroed.infoOnly')">
                {{ $t('delegateFollowup.unzeroed.ge100') }}
                <strong class="df-num">{{ summary.remainingGe100Count }}</strong>
              </span>
              <span class="df-chip static" :title="$t('delegateFollowup.unzeroed.infoOnly')">
                {{ $t('delegateFollowup.unzeroed.newUnpaid') }}
                <strong class="df-num">{{ summary.newSaleUnpaid3DaysCount }}</strong>
              </span>
            </div>
          </div>
        </section>

        <!-- Conversion -->
        <section class="df-section" aria-labelledby="df-conv-title">
          <h2 id="df-conv-title" class="df-section-title">
            {{ $t('delegateFollowup.conversion.title') }}
          </h2>
          <div class="df-chip-row">
            <span class="df-chip static">
              {{ $t('delegateFollowup.conversion.improved') }}
              <strong class="df-num">{{ summary.conversion.improved }}</strong>
            </span>
            <span class="df-chip static">
              {{ $t('delegateFollowup.conversion.declined') }}
              <strong class="df-num">{{ summary.conversion.declined }}</strong>
            </span>
            <span class="df-chip static">
              {{ $t('delegateFollowup.conversion.unchanged') }}
              <strong class="df-num">{{ summary.conversion.unchanged }}</strong>
            </span>
          </div>
          <p v-if="summary.conversion.summaryText" class="df-muted">
            {{ summary.conversion.summaryText }}
          </p>
          <p v-if="!summary.conversion.legalTargetAvailable" class="df-muted">
            {{ $t('delegateFollowup.conversion.legalUnavailable') }}
          </p>
          <div v-if="!summary.conversion.targets.length" class="df-empty">
            {{ $t('delegateFollowup.conversion.noTargets') }}
          </div>
          <div v-else class="df-tiles">
            <article
              v-for="tg in summary.conversion.targets"
              :key="tg.targetLabel"
              class="df-conv-card"
            >
              <div class="df-card-top">
                <span class="df-badge" :class="tierClass(tg.targetLabel)">
                  {{ $t('delegateFollowup.conversion.arrived') }} {{ tg.targetLabel }}
                </span>
                <strong class="df-tile-value df-num">{{ tg.arrivedCount }}</strong>
              </div>
              <p v-if="tg.detailText" class="df-tile-note">{{ tg.detailText }}</p>
              <div v-if="tg.fromBreakdown.length" class="df-chip-row">
                <span
                  v-for="f in tg.fromBreakdown"
                  :key="f.fromLabel"
                  class="df-chip static"
                >
                  {{ $t('delegateFollowup.conversion.from') }} {{ f.fromLabel }}
                  <strong class="df-num">{{ f.count }}</strong>
                </span>
              </div>
              <template v-if="tg.customers.length">
                <button
                  type="button"
                  class="df-btn ghost small"
                  :aria-expanded="isConvOpen(tg.targetLabel)"
                  @click="toggleConv(tg.targetLabel)"
                >
                  {{
                    isConvOpen(tg.targetLabel)
                      ? $t('delegateFollowup.conversion.hideCustomers')
                      : $t('delegateFollowup.conversion.showCustomers')
                  }}
                  (<span class="df-num">{{ tg.customers.length }}</span>)
                </button>
                <ul v-if="isConvOpen(tg.targetLabel)" class="df-conv-list">
                  <li v-for="c in tg.customers" :key="c.customerId">
                    <button
                      v-if="canOpenProfile"
                      type="button"
                      class="df-link"
                      @click="openProfile(c.customerId)"
                    >
                      {{ c.customerName }}
                    </button>
                    <span v-else>{{ c.customerName }}</span>
                    <span class="df-muted">
                      — {{ $t('delegateFollowup.conversion.from') }} {{ c.fromLabel }}
                    </span>
                  </li>
                </ul>
              </template>
              <button
                type="button"
                class="df-btn ghost small"
                @click="openConversionHistory(tg)"
              >
                {{ $t('delegateFollowup.conversion.history') }}
              </button>
            </article>
          </div>
        </section>

        <!-- Customers table -->
        <section
          id="df-customers"
          ref="tableSection"
          class="df-section"
          aria-labelledby="df-customers-title"
        >
          <div class="df-section-head">
            <h2 id="df-customers-title" class="df-section-title">
              {{ $t('delegateFollowup.customers.title') }}
            </h2>
            <span v-if="showTable && customersData" class="df-muted">
              {{ $t('delegateFollowup.customers.shown') }}
              <span class="df-num">{{ visibleCustomers.length }}</span>
              {{ $t('delegateFollowup.customers.of') }}
              <span class="df-num">{{ filteredCustomers.length }}</span>
            </span>
          </div>

          <p v-if="!showTable" class="df-muted">
            {{ $t('delegateFollowup.customers.hiddenHint') }}
          </p>

          <template v-else>
            <div class="df-filters" role="group" :aria-label="$t('delegateFollowup.customers.title')">
              <button
                v-for="f in filterButtons"
                :key="f.id"
                type="button"
                class="df-filter"
                :aria-pressed="tableFilter === f.id"
                :disabled="f.disabled"
                @click="setFilter(f.id)"
              >
                {{ f.label }}
              </button>
              <input
                v-model="search"
                type="search"
                class="df-input df-search"
                :placeholder="$t('delegateFollowup.customers.search')"
                :aria-label="$t('delegateFollowup.customers.search')"
              />
            </div>
            <p v-if="!legalAvailable" class="df-muted">
              {{ $t('delegateFollowup.customers.legalUnavailable') }}
            </p>

            <div v-if="customersQuery.isLoading.value" class="df-muted" role="status">
              {{ $t('delegateFollowup.customers.loading') }}
            </div>
            <div v-else-if="customersQuery.isError.value" class="df-error" role="alert">
              <span>{{ $t('delegateFollowup.customers.error') }}</span>
              <button type="button" class="df-btn ghost" @click="customersQuery.refetch()">
                {{ $t('delegateFollowup.lists.retry') }}
              </button>
            </div>
            <template v-else-if="customersData">
              <div v-if="!filteredCustomers.length" class="df-empty">
                {{ $t('delegateFollowup.customers.empty') }}
              </div>
              <div v-else class="df-table-wrap">
                <table class="df-table customers">
                  <thead>
                    <tr>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.name') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.phone') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.shop') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.address') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.sale') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.delegate') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.follower') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.saleDate') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.totalSales') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.daySales') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.receipts') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.remaining') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.items') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.lastPayment') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.lastAmount') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.receiptCount') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.days') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.rate') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.score') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.label') }}</th>
                      <th scope="col">{{ $t('delegateFollowup.customers.cols.flags') }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="c in visibleCustomers" :key="c.customerId">
                      <td class="wrap">
                        <button
                          v-if="canOpenProfile"
                          type="button"
                          class="df-link"
                          :title="$t('delegateFollowup.customers.profile')"
                          @click="openProfile(c.customerId)"
                        >
                          {{ c.customerName }}
                        </button>
                        <span v-else :title="$t('delegateFollowup.customers.profileUnavailable')">
                          {{ c.customerName }}
                        </span>
                      </td>
                      <td class="nowrap"><span class="df-num">{{ c.phoneNumber ?? '—' }}</span></td>
                      <td class="wrap">{{ c.shopName ?? '—' }}</td>
                      <td class="wrap">{{ c.address ?? '—' }}</td>
                      <td class="wrap">{{ c.saleName ?? '—' }}</td>
                      <td class="wrap">{{ c.delegateName ?? '—' }}</td>
                      <td class="wrap">{{ c.followerName ?? '—' }}</td>
                      <td class="nowrap"><span class="df-num">{{ c.dateSaleDevice ?? '—' }}</span></td>
                      <td class="nowrap">{{ fmtMoney(c.amountTotalSales) }}</td>
                      <td class="nowrap">{{ fmtMoney(c.amountDaySales) }}</td>
                      <td class="nowrap">{{ fmtMoney(c.receiptsTotal) }}</td>
                      <td class="nowrap">{{ fmtMoney(c.amountRemaining) }}</td>
                      <td class="wrap">{{ c.itemsNames ?? '—' }}</td>
                      <td class="nowrap"><span class="df-num">{{ c.lastPaymentDate ?? '—' }}</span></td>
                      <td class="nowrap">
                        {{ c.lastPaymentAmount != null ? fmtMoney(c.lastPaymentAmount) : '—' }}
                      </td>
                      <td class="nowrap"><span class="df-num">{{ c.countReceiptDevice }}</span></td>
                      <td class="nowrap"><span class="df-num">{{ c.numberOfDayDevice }}</span></td>
                      <td class="nowrap"><span class="df-num">{{ fmtNum(c.receiptRateDevice) }}</span></td>
                      <td class="nowrap"><span class="df-num">{{ c.score ?? '—' }}</span></td>
                      <td class="nowrap">
                        <span class="df-badge" :class="tierClass(c.realRatingLabel)">
                          {{ c.realRatingLabel || '—' }}
                        </span>
                      </td>
                      <td class="wrap">
                        <span v-if="c.isNewSaleUnpaid3Days" class="df-badge tier-lagging">
                          {{ $t('delegateFollowup.customers.newUnpaid') }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div v-if="visibleCustomers.length < filteredCustomers.length" class="df-actions">
                <button type="button" class="df-btn outline" @click="visibleLimit += PAGE_SIZE">
                  {{ $t('delegateFollowup.customers.showMore') }}
                </button>
              </div>
              <aside v-if="customerNotes.length" class="df-banner" role="note">
                <ul>
                  <li v-for="(n, i) in customerNotes" :key="i">
                    <strong>{{ n.field }}</strong>: {{ n.note }}
                  </li>
                </ul>
              </aside>
            </template>
          </template>
        </section>
      </div>
    </template>

    <BoardRatingDialog
      :visible="rateOpen"
      :busy="upsertMutation.isPending.value || submitting"
      :listName="summary?.listName"
      :initialScore="summary?.boardOperatorRating.hasRating ? summary.boardOperatorRating.score : null"
      :initialNote="summary?.boardOperatorRating.hasRating ? summary.boardOperatorRating.note : null"
      @close="rateOpen = false"
      @submit="submitRating"
    />

    <DelegateHistoryDialog
      :visible="history != null"
      :title="history?.title ?? ''"
      :subtitle="history?.subtitle"
      :columns="history?.columns ?? []"
      :rows="history?.rows ?? []"
      @close="history = null"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import BoardRatingDialog from '../components/BoardRatingDialog.vue'
import DelegateHistoryDialog, {
  type HistoryColumn,
  type HistoryRow,
} from '../components/DelegateHistoryDialog.vue'
import '../components/delegate-followup.css'
import { useListsQuery } from '../requests/queries/useListsQuery'
import { useSummaryQuery } from '../requests/queries/useSummaryQuery'
import { useCustomersQuery } from '../requests/queries/useCustomersQuery'
import { useUpsertBoardRatingMutation } from '../requests/mutations/useUpsertBoardRatingMutation'
import {
  BRANCH_STORAGE_KEY,
  type DelegateFollowupCommitteeRating,
  type DelegateFollowupConversionTarget,
  type DelegateFollowupCustomer,
  type DelegateFollowupTierMoney,
} from '../domain/types'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { useToastStore } from '@/modules/Core/store'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'

type TableFilter = 'all' | 'tiers' | 'lt100' | '100to200' | 'legal'

const PAGE_SIZE = 100
const LIST_STORAGE_KEY = 'delegate-followup-list'
const BAND_100K = 100_000
const BAND_200K = 200_000
const LEGAL_LABEL = 'قانونية'
const TIER_LABELS = ['ممتاز', 'جيد', 'ضعيف', 'متلكئ', 'سيء']

const { t } = useI18n()
const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const { can, canAny } = usePermission()

/* ---------- branch ---------- */
const branches = computed(() => auth.branches ?? [])

function readStoredBranch(): number | null {
  try {
    const raw = sessionStorage.getItem(BRANCH_STORAGE_KEY)
    const id = raw == null ? NaN : Number(raw)
    return Number.isFinite(id) && id > 0 ? id : null
  } catch {
    return null
  }
}

function resolveInitialBranch(): number | null {
  const list = branches.value
  const stored = readStoredBranch()
  if (stored != null && list.some((b) => b.id === stored)) return stored
  if (list.length === 1) return list[0].id
  return null
}

const branchId = ref<number | null>(resolveInitialBranch())
const listId = ref<number | null>(null)

const currentBranch = computed(
  () => branches.value.find((b) => b.id === branchId.value) ?? null
)

function pickBranch(id: number) {
  branchId.value = id
  listId.value = null
  try {
    sessionStorage.setItem(BRANCH_STORAGE_KEY, String(id))
    sessionStorage.removeItem(LIST_STORAGE_KEY)
  } catch {
    /* storage unavailable */
  }
}

function changeBranch() {
  closeDetailState()
  listId.value = null
  branchId.value = null
}

watch(branches, (list) => {
  if (branchId.value != null && list.some((b) => b.id === branchId.value) === false) {
    changeBranch()
  }
  if (branchId.value == null && list.length === 1) pickBranch(list[0].id)
})

watch(
  () => auth.branchGeneration,
  () => {
    closeDetailState()
    listId.value = null
  }
)

/* ---------- queries ---------- */
const listsQuery = useListsQuery(branchId)
const lists = computed(() => listsQuery.data.value?.lists ?? [])

const summaryQuery = useSummaryQuery(listId, branchId)
const customersQuery = useCustomersQuery(listId, branchId)
const summary = computed(() => summaryQuery.data.value ?? null)
const customersData = computed(() => customersQuery.data.value ?? null)

const selectedListName = computed(
  () => lists.value.find((l) => l.listId === listId.value)?.listName ?? ''
)

function openList(id: number) {
  closeDetailState()
  listId.value = id
  try {
    sessionStorage.setItem(LIST_STORAGE_KEY, `${branchId.value}:${id}`)
  } catch {
    /* storage unavailable */
  }
}

function backToLists() {
  closeDetailState()
  listId.value = null
  try {
    sessionStorage.removeItem(LIST_STORAGE_KEY)
  } catch {
    /* storage unavailable */
  }
}

function openFullCustomersPage() {
  if (branchId.value == null || listId.value == null) return
  void router.push({
    name: 'ManagementDirectorDelegateFollowupCustomersRoute',
    query: {
      branchId: String(branchId.value),
      listId: String(listId.value),
      filter: tableFilter.value !== 'all' ? tableFilter.value : undefined,
      search: search.value.trim() || undefined,
    },
  })
}

function reloadDetail() {
  void summaryQuery.refetch()
  void customersQuery.refetch()
}

/* ---------- unresolved notes ---------- */
const unresolvedNotes = computed(() => summary.value?.unresolvedNotes ?? [])
const customerNotes = computed(() => {
  const seen = new Set(unresolvedNotes.value.map((n) => `${n.field}|${n.note}`))
  return (customersData.value?.unresolvedNotes ?? []).filter(
    (n) => seen.has(`${n.field}|${n.note}`) === false
  )
})

/* ---------- permissions ---------- */
const canRate = computed(() => can('DelegateFollowup', 'UpsertBoardRating'))
const canOpenProfile = computed(() =>
  canAny([
    { module: 'Customer', action: 'Read' },
    { module: 'SalesRequest', action: 'ReadAuditProfile' },
    { module: 'SalesRequest', action: 'MatchRead' },
  ])
)

/* ---------- formatting ---------- */
function fmtNum(n: number | null | undefined): string {
  if (n == null || Number.isFinite(n) === false) return '—'
  return n.toLocaleString('en-US', { maximumFractionDigits: 2 })
}

function fmtMoney(n: number | null | undefined): string {
  if (n == null || Number.isFinite(n) === false) return '—'
  return `${n.toLocaleString('en-US', { maximumFractionDigits: 0 })} ${t('delegateFollowup.currency')}`
}

function tierClass(label: string | null | undefined): string {
  switch ((label ?? '').trim()) {
    case 'ممتاز':
      return 'tier-excellent'
    case 'جيد':
      return 'tier-good'
    case 'ضعيف':
      return 'tier-weak'
    case 'متلكئ':
      return 'tier-lagging'
    case 'سيء':
      return 'tier-bad'
    case LEGAL_LABEL:
      return 'tier-legal'
    default:
      return 'tier-unknown'
  }
}

/* ---------- ratings ---------- */
const trendKey = computed(() => {
  const v = summary.value?.ratingTrend
  return v === 'up' || v === 'down' ? v : 'flat'
})
const trendIcon = computed(() =>
  trendKey.value === 'up' ? '▲' : trendKey.value === 'down' ? '▼' : '■'
)
const trendText = computed(() =>
  t(
    trendKey.value === 'up'
      ? 'delegateFollowup.ratings.trendUp'
      : trendKey.value === 'down'
        ? 'delegateFollowup.ratings.trendDown'
        : 'delegateFollowup.ratings.trendFlat'
  )
)

const committees = computed(() => {
  const s = summary.value
  if (s == null) return []
  return [
    {
      key: 'follower',
      label: t('delegateFollowup.ratings.followerCommittee'),
      data: s.followerCommittee,
    },
    {
      key: 'supervision',
      label: t('delegateFollowup.ratings.supervisionCommittee'),
      data: s.supervisionCommittee,
    },
    {
      key: 'delegate',
      label: t('delegateFollowup.ratings.salesDelegate'),
      data: s.salesDelegate,
    },
  ]
})

/* ---------- board rating ---------- */
const rateOpen = ref(false)
const submitting = ref(false)
const upsertMutation = useUpsertBoardRatingMutation()

async function submitRating(payload: { score: number; note: string | null }) {
  if (
    submitting.value ||
    upsertMutation.isPending.value ||
    canRate.value === false ||
    listId.value == null ||
    branchId.value == null
  ) {
    return
  }
  submitting.value = true
  try {
    await upsertMutation.mutateAsync({
      listId: listId.value,
      branchId: branchId.value,
      score: payload.score,
      note: payload.note,
    })
    rateOpen.value = false
    toast.setMassage({
      title: t('delegateFollowup.dialog.saved'),
      dialogState: DialogState.Success,
      isOpen: true,
    })
  } catch {
    /* error toast is shown by the axios interceptor */
  } finally {
    submitting.value = false
  }
}

/* ---------- unzeroed chips + table filter ---------- */
const legalAvailable = computed(
  () =>
    summary.value?.legalAvailable === true &&
    customersData.value?.legalAvailable !== false
)

const tierChips = computed(() => {
  const s = summary.value
  if (s == null) return []
  return [
    { label: 'ممتاز', count: s.excellentCount },
    { label: 'جيد', count: s.goodCount },
    { label: 'ضعيف', count: s.weakCount },
    { label: 'متلكئ', count: s.laggingCount },
    { label: 'سيء', count: s.badCount },
  ]
})

const showTable = ref(false)
const tableFilter = ref<TableFilter>('all')
const tierFilter = ref<string | null>(null)
const search = ref('')
const visibleLimit = ref(PAGE_SIZE)
const tableSection = ref<HTMLElement | null>(null)

function closeDetailState() {
  showTable.value = false
  tableFilter.value = 'all'
  tierFilter.value = null
  search.value = ''
  visibleLimit.value = PAGE_SIZE
  rateOpen.value = false
  history.value = null
  convOpen.value = {}
}

async function revealTable() {
  showTable.value = true
  await nextTick()
  tableSection.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function toggleTable() {
  if (showTable.value) {
    showTable.value = false
    return
  }
  void revealTable()
}

function setFilter(f: TableFilter) {
  if (f === 'legal' && legalAvailable.value === false) return
  tableFilter.value = f
  if (f !== 'tiers') tierFilter.value = null
}

function toggleFilter(f: TableFilter) {
  if (f === 'legal' && legalAvailable.value === false) return
  if (tableFilter.value === f) {
    setFilter('all')
    return
  }
  setFilter(f)
  void revealTable()
}

function toggleTierChip(label: string) {
  if (tableFilter.value === 'tiers' && tierFilter.value === label) {
    setFilter('all')
    return
  }
  tableFilter.value = 'tiers'
  tierFilter.value = label
  void revealTable()
}

const filterButtons = computed(() => [
  { id: 'all' as const, label: t('delegateFollowup.customers.filters.all'), disabled: false },
  { id: 'tiers' as const, label: t('delegateFollowup.customers.filters.tiers'), disabled: false },
  { id: 'lt100' as const, label: t('delegateFollowup.customers.filters.lt100'), disabled: false },
  { id: '100to200' as const, label: t('delegateFollowup.customers.filters.100to200'), disabled: false },
  {
    id: 'legal' as const,
    label: t('delegateFollowup.customers.filters.legal'),
    disabled: legalAvailable.value === false,
  },
])

function matchesFilter(c: DelegateFollowupCustomer): boolean {
  switch (tableFilter.value) {
    case 'tiers':
      if (c.isLegal) return false
      return tierFilter.value != null
        ? c.realRatingLabel === tierFilter.value
        : TIER_LABELS.includes(c.realRatingLabel)
    case 'lt100':
      return c.amountRemaining > 0 && c.amountRemaining < BAND_100K
    case '100to200':
      return c.amountRemaining >= BAND_100K && c.amountRemaining < BAND_200K
    case 'legal':
      return c.isLegal === true || c.realRatingLabel === LEGAL_LABEL
    default:
      return true
  }
}

const filteredCustomers = computed(() => {
  const rows = customersData.value?.customers ?? []
  const q = search.value.trim().toLowerCase()
  return rows.filter((c) => {
    if (matchesFilter(c) === false) return false
    if (q === '') return true
    return [c.customerName, c.phoneNumber, c.shopName]
      .filter((v): v is string => typeof v === 'string')
      .some((v) => v.toLowerCase().includes(q))
  })
})

const visibleCustomers = computed(() =>
  filteredCustomers.value.slice(0, visibleLimit.value)
)

watch([tableFilter, tierFilter, search], () => {
  visibleLimit.value = PAGE_SIZE
})

watch(legalAvailable, (v) => {
  if (v === false && tableFilter.value === 'legal') setFilter('all')
})

/* ---------- conversion ---------- */
const convOpen = ref<Record<string, boolean>>({})
function isConvOpen(label: string) {
  return convOpen.value[label] === true
}
function toggleConv(label: string) {
  convOpen.value = { ...convOpen.value, [label]: !isConvOpen(label) }
}

/* ---------- customer profile ---------- */
function openProfile(customerId: number) {
  if (canOpenProfile.value === false) return
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: customerId },
    query: {
      returnTo: 'ManagementDirectorDelegateFollowupRoute',
      branchId: branchId.value != null ? String(branchId.value) : undefined,
    },
  })
}

/* ---------- history dialogs ---------- */
interface HistoryState {
  title: string
  subtitle?: string
  columns: HistoryColumn[]
  rows: HistoryRow[]
}
const history = ref<HistoryState | null>(null)

function dateCol(): HistoryColumn {
  return {
  key: 'date',
  label: t('delegateFollowup.dialog.date'),
  num: true,
  }
}

function amountRows(
  items: Array<{ date: string; amount: number }>
): HistoryRow[] {
  return items.map((i) => ({ date: i.date, amount: fmtMoney(i.amount) }))
}

function openRealHistory() {
  const s = summary.value
  if (s == null) return
  history.value = {
    title: t('delegateFollowup.ratings.real'),
    subtitle: s.listName,
    columns: [
      dateCol(),
      { key: 'score', label: t('delegateFollowup.dialog.score'), num: true },
      { key: 'label', label: t('delegateFollowup.dialog.label') },
    ],
    rows: s.realRatingLast30Days.map((r) => ({
      date: r.date,
      score: fmtNum(r.score),
      label: r.label,
    })),
  }
}

function openBoardHistory() {
  const s = summary.value
  if (s == null) return
  history.value = {
    title: t('delegateFollowup.ratings.board'),
    subtitle: s.listName,
    columns: [
      dateCol(),
      { key: 'score', label: t('delegateFollowup.dialog.boardScore'), num: true },
      { key: 'note', label: t('delegateFollowup.dialog.boardNote'), wrap: true },
    ],
    rows: s.boardOperatorLast30Days.map((r) => ({
      date: r.date,
      score: r.boardOperatorScore ?? fmtNum(r.score),
      note: r.boardOperatorNote,
    })),
  }
}

function openCommitteeHistory(
  label: string,
  data: DelegateFollowupCommitteeRating
) {
  history.value = {
    title: label,
    subtitle: summary.value?.listName,
    columns: [
      dateCol(),
      { key: 'score', label: t('delegateFollowup.dialog.score'), num: true },
      { key: 'label', label: t('delegateFollowup.dialog.label') },
    ],
    rows: data.last30Days.map((r) => ({
      date: r.date,
      score: fmtNum(r.score),
      label: r.label,
    })),
  }
}

function openDaysHistory() {
  const s = summary.value
  if (s == null) return
  history.value = {
    title: t('delegateFollowup.income.historyDays'),
    subtitle: s.listName,
    columns: [
      dateCol(),
      { key: 'amount', label: t('delegateFollowup.dialog.amount') },
    ],
    rows: amountRows(s.collectedLast30Days),
  }
}

function openFridaysHistory() {
  const s = summary.value
  if (s == null) return
  history.value = {
    title: t('delegateFollowup.income.historyFridays'),
    subtitle: s.listName,
    columns: [
      dateCol(),
      { key: 'amount', label: t('delegateFollowup.dialog.amount') },
    ],
    rows: amountRows(s.collectedLast30Fridays),
  }
}

function openTierHistory(tier: DelegateFollowupTierMoney) {
  history.value = {
    title: `${t('delegateFollowup.income.tier')}: ${tier.label}`,
    subtitle: summary.value?.listName,
    columns: [
      dateCol(),
      { key: 'amount', label: t('delegateFollowup.dialog.amount') },
    ],
    rows: amountRows(tier.collectedLast30Days),
  }
}

function openConversionHistory(target: DelegateFollowupConversionTarget) {
  history.value = {
    title: `${t('delegateFollowup.conversion.arrived')} ${target.targetLabel}`,
    subtitle: target.detailText || summary.value?.listName,
    columns: [
      dateCol(),
      { key: 'arrived', label: t('delegateFollowup.dialog.arrived'), num: true },
      { key: 'from', label: t('delegateFollowup.dialog.from'), wrap: true },
      { key: 'customers', label: t('delegateFollowup.dialog.customers'), wrap: true },
    ],
    rows: target.last30Days.map((d) => ({
      date: d.date,
      arrived: d.arrivedCount,
      from:
        d.fromBreakdown.map((f) => `${f.fromLabel} (${f.count})`).join('، ') ||
        '—',
      customers:
        d.customers
          .map((c) => `${c.customerName} (${c.fromLabel})`)
          .join('، ') || '—',
    })),
  }
}
/* restore last list (e.g. after returning from a customer profile) */
let restoredList = false
watch(
  lists,
  (value) => {
    if (restoredList || value.length === 0 || listId.value != null) return
    restoredList = true
    try {
      const raw = sessionStorage.getItem(LIST_STORAGE_KEY)
      if (raw == null) return
      const [b, l] = raw.split(':').map(Number)
      if (b === branchId.value && value.some((x) => x.listId === l)) openList(l)
    } catch {
      /* storage unavailable */
    }
  },
  { immediate: true }
)
</script>
