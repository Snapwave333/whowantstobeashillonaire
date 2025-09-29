#!/usr/bin/env python3
"""
AI Image Generator for Who Wants to Be a Shillonair
Uses Stability AI API to generate consistent crypto-themed assets
"""

import requests
import json
import os
import base64
from pathlib import Path

# Stability AI Configuration
API_KEY = "sk-ZTJcSFjCb7Oq7GIiMbgeY6DTchxErKoEkBscalWflDYtrEMR"
API_HOST = "https://api.stability.ai"
ENGINE_ID = "stable-diffusion-xl-1024-v1-0"

# Base style prompt for consistency
BASE_STYLE = "futuristic crypto broadcast overlay, deep space black background with subtle starfield, neon green accents (#00ff88), gold highlights (#ffd700), electric blue (#00bfff), clean tech aesthetic, high-contrast, soft glow effects, professional UI design, sharp and readable"

# Image specifications (SDXL valid dimensions: 1024x1024, 1152x896, 1216x832, 1344x768, 1536x640, 640x1536, 768x1344, 832x1216, 896x1152)
IMAGES_TO_GENERATE = [
    # Backgrounds (1536x640 - wide format for overlays)
    {
        "filename": "assets/images/background/space_default.jpg",
        "prompt": f"ultra-wide space wallpaper, {BASE_STYLE}, minimal clutter, unobtrusive center region for overlay content, subtle geometric crypto motifs, starfield background",
        "width": 1536,
        "height": 640,
        "format": "jpeg"
    },
    {
        "filename": "assets/images/background/crypto_grid.png",
        "prompt": f"transparent grid overlay pattern, {BASE_STYLE}, subtle geometric grid lines, cryptocurrency circuit board pattern, transparent background",
        "width": 1536,
        "height": 640,
        "format": "png"
    },
    {
        "filename": "assets/images/background/neon_circuit.webp",
        "prompt": f"neon circuit board background, {BASE_STYLE}, glowing circuit traces, cryptocurrency symbols integrated into circuits, dark background with bright neon lines",
        "width": 1536,
        "height": 640,
        "format": "webp"
    },
    
    # Crypto Icons (1024x1024 - square format)
    {
        "filename": "assets/images/icons/btc.png",
        "prompt": f"Bitcoin BTC logo icon, {BASE_STYLE}, flat design, neon orange glow, transparent background, clean minimal design",
        "width": 1024,
        "height": 1024,
        "format": "png"
    },
    {
        "filename": "assets/images/icons/eth.png",
        "prompt": f"Ethereum ETH logo icon, {BASE_STYLE}, flat design, neon blue glow, transparent background, clean minimal design",
        "width": 1024,
        "height": 1024,
        "format": "png"
    },
    {
        "filename": "assets/images/icons/shill.png",
        "prompt": f"SHILL cryptocurrency logo icon, {BASE_STYLE}, flat design, neon green glow, transparent background, clean minimal design, stylized S symbol",
        "width": 1024,
        "height": 1024,
        "format": "png"
    },
    
    # Lifeline Icons (1024x1024 - square format)
    {
        "filename": "assets/images/icons/lifeline_50_50.png",
        "prompt": f"50/50 lifeline icon, {BASE_STYLE}, split circle design, half green half red, neon glow effect, transparent background",
        "width": 1024,
        "height": 1024,
        "format": "png"
    },
    {
        "filename": "assets/images/icons/lifeline_discord.png",
        "prompt": f"Discord chat icon, {BASE_STYLE}, speech bubble with Discord logo, neon purple glow, transparent background",
        "width": 1024,
        "height": 1024,
        "format": "png"
    },
    {
        "filename": "assets/images/icons/lifeline_phone.png",
        "prompt": f"Phone call icon, {BASE_STYLE}, retro phone handset, neon blue glow, transparent background",
        "width": 1024,
        "height": 1024,
        "format": "png"
    },
    
    # UI Elements (1152x896 - button format)
    {
        "filename": "assets/images/ui/button_normal.png",
        "prompt": f"UI button normal state, {BASE_STYLE}, rounded rectangle, subtle gradient, neon border, transparent background",
        "width": 1152,
        "height": 896,
        "format": "png"
    },
    {
        "filename": "assets/images/ui/button_hover.png",
        "prompt": f"UI button hover state, {BASE_STYLE}, rounded rectangle, bright glow effect, neon border, transparent background",
        "width": 1152,
        "height": 896,
        "format": "png"
    },
    
    # Custom Assets (1216x832 - landscape format)
    {
        "filename": "assets/images/custom/reveals/tier_5_win.jpg",
        "prompt": f"celebration image for tier 5 victory, {BASE_STYLE}, golden confetti, cryptocurrency symbols, victory celebration, $1000 prize theme",
        "width": 1216,
        "height": 832,
        "format": "jpeg"
    },
    {
        "filename": "assets/images/custom/reveals/tier_10_bonus.png",
        "prompt": f"celebration image for tier 10 bonus, {BASE_STYLE}, electric blue celebration, safe haven reached, cryptocurrency rain effect",
        "width": 1216,
        "height": 832,
        "format": "png"
    },
    {
        "filename": "assets/images/custom/reveals/tier_15_victory.jpg",
        "prompt": f"ultimate victory celebration for tier 15, {BASE_STYLE}, massive golden explosion, 1 BTC prize, ultimate crypto victory, fireworks and sparkles",
        "width": 1216,
        "height": 832,
        "format": "jpeg"
    },
    
    # Host Avatar (1024x1024 - square format)
    {
        "filename": "assets/images/custom/host_avatar.png",
        "prompt": f"professional crypto host avatar, {BASE_STYLE}, circular crop friendly, futuristic host character, neon accents, professional appearance",
        "width": 1024,
        "height": 1024,
        "format": "png"
    }
]

def generate_image(prompt, width, height, filename):
    """Generate a single image using Stability AI API"""
    
    # Ensure directory exists
    os.makedirs(os.path.dirname(filename), exist_ok=True)
    
    # API request
    response = requests.post(
        f"{API_HOST}/v1/generation/{ENGINE_ID}/text-to-image",
        headers={
            "Content-Type": "application/json",
            "Accept": "application/json",
            "Authorization": f"Bearer {API_KEY}"
        },
        json={
            "text_prompts": [
                {
                    "text": prompt,
                    "weight": 1
                }
            ],
            "cfg_scale": 7,
            "height": height,
            "width": width,
            "samples": 1,
            "steps": 30,
        },
    )
    
    if response.status_code != 200:
        print(f"Error generating {filename}: {response.status_code}")
        print(response.text)
        return False
    
    # Save the image
    data = response.json()
    
    for i, image in enumerate(data["artifacts"]):
        with open(filename, "wb") as f:
            f.write(base64.b64decode(image["base64"]))
        print(f"✓ Generated: {filename}")
        return True
    
    return False

def main():
    """Generate all images"""
    print("🚀 Starting AI image generation for Who Wants to Be a Shillonair")
    print(f"📁 Creating {len(IMAGES_TO_GENERATE)} images...")
    
    success_count = 0
    
    for image_spec in IMAGES_TO_GENERATE:
        print(f"\n🎨 Generating: {image_spec['filename']}")
        print(f"📐 Size: {image_spec['width']}x{image_spec['height']}")
        
        if generate_image(
            image_spec['prompt'],
            image_spec['width'],
            image_spec['height'],
            image_spec['filename']
        ):
            success_count += 1
        else:
            print(f"❌ Failed to generate: {image_spec['filename']}")
    
    print(f"\n🎉 Generation complete!")
    print(f"✅ Successfully generated: {success_count}/{len(IMAGES_TO_GENERATE)} images")
    
    if success_count == len(IMAGES_TO_GENERATE):
        print("🎊 All images generated successfully!")
    else:
        print(f"⚠️  {len(IMAGES_TO_GENERATE) - success_count} images failed to generate")

if __name__ == "__main__":
    main()