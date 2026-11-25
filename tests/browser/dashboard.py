"""Exercise the bilingual dashboard against a fresh local Compose stack."""
import os
import secrets
from pathlib import Path
from playwright.sync_api import sync_playwright

url = "http://localhost:8080"
password = secrets.token_urlsafe(24)
with sync_playwright() as driver:
    browser = driver.chromium.launch(executable_path=os.environ.get("CHROME_BIN"))
    page = browser.new_page(viewport={"width": 1280, "height": 800})
    response = page.request.post(f"{url}/api/auth/register", data={
        "username": "workspace-check", "email": "workspace@example.invalid",
        "password": password, "firstName": "Smoke", "lastName": "Test"
    })
    assert response.ok, f"Registration returned HTTP {response.status}"
    page.goto(url)
    page.select_option("#language", "uk")
    page.reload()
    page.get_by_role("heading", name="Керування складом").wait_for()
    page.fill("#username", "workspace-check")
    page.fill("#password", password)
    page.click("#login button")
    page.locator("#metrics strong").first.wait_for()
    assert int(page.locator("#metrics strong").first.inner_text()) > 0
    page.select_option("#language", "en")
    page.get_by_role("heading", name="Inventory overview").wait_for()
    Path("artifacts").mkdir(exist_ok=True)
    page.screenshot(path="artifacts/warehouse.png", full_page=True)
    page.set_viewport_size({"width": 375, "height": 812})
    assert page.evaluate("document.documentElement.scrollWidth <= innerWidth")
    page.click("#logout")
    page.get_by_role("heading", name="Sign in").wait_for()
    browser.close()
